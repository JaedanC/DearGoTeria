using System;
using Godot;
using ImGuiNET;
using System.Linq;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Generic = System.Collections.Generic;

public class ImGuiInterface : ImGuiNode
{
    private Generic.Queue<float>? processDeltas;
    private Generic.Queue<float>? physicsProcessDeltas;
    private Generic.Queue<float>? staticMemory;
    private Generic.Queue<float>? dynamicMemory;
    private int threshold = 1000;
    private int memoryThreshold = 500;

    private ViewportContainer? gameContainer;
    private ViewportContainer? imguiContainer;
    private Viewport? gameViewport;
    private Godot.Vector2 gameWindowPosition = Godot.Vector2.Zero;
    private Node? scene;
    private Rect2 gameViewportRect;
    private bool mouseInsideGame;

    private Generic.List<ImGuiLogData>? imguiLogData;


    public override void _Ready()
    {
        base._Ready();
        processDeltas = new Generic.Queue<float>(threshold);
        physicsProcessDeltas = new Generic.Queue<float>(threshold);
        staticMemory = new Generic.Queue<float>(memoryThreshold);
        dynamicMemory = new Generic.Queue<float>(memoryThreshold);
        processDeltas.Enqueue(0);
        physicsProcessDeltas.Enqueue(0);
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        gameContainer = GetTree().Root.GetChild(0).GetChild<ViewportContainer>(0);
        gameViewport = gameContainer.GetChild<Viewport>(0);
        scene = gameViewport.GetChild(0);
        imguiContainer = GetTree().Root.GetChild(0).GetChild<ViewportContainer>(1);
        Assert.True(gameViewport.RenderTargetVFlip);

        imguiLogData = new Generic.List<ImGuiLogData>
        {
            new ImGuiLogData
            {
                LogLevel = ImGuiLog.LogType.Debug,
                Colour = Extensions.ColorToFloat(new Vector4(85, 185, 255, 255)),
                Function = ImGuiLog.Debug
            },
            new ImGuiLogData
            {
                LogLevel = ImGuiLog.LogType.Info,
                Colour = Vector4.One,
                Function = ImGuiLog.Info
            },
            new ImGuiLogData
            {
                LogLevel = ImGuiLog.LogType.Warning,
                Colour = Extensions.ColorToFloat(new Vector4(255, 164, 63, 255)),
                Function = ImGuiLog.Warning
            },
            new ImGuiLogData
            {
                LogLevel = ImGuiLog.LogType.Critical,
                Colour = Extensions.ColorToFloat(new Vector4(255, 64, 64, 255)),
                Function = ImGuiLog.Critical
            }
        };
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        processDeltas!.Enqueue(delta);
        while (processDeltas.Count > threshold)
        {
            processDeltas.Dequeue();
        }

        gameViewportRect = gameViewport!.GetVisibleRect();
        gameViewportRect.Position += gameContainer!.RectPosition;
        mouseInsideGame = gameViewportRect.HasPoint(GetViewport().GetMousePosition());
        imguiContainer!.MouseFilter = mouseInsideGame ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        physicsProcessDeltas!.Enqueue(delta);
        while (physicsProcessDeltas.Count > threshold)
        {
            physicsProcessDeltas.Dequeue();
        }

        staticMemory!.Enqueue(Performance.GetMonitor(Performance.Monitor.MemoryStatic));
        dynamicMemory!.Enqueue(Performance.GetMonitor(Performance.Monitor.MemoryDynamic));
        while (staticMemory.Count > memoryThreshold)
        {
            staticMemory.Dequeue();
            dynamicMemory.Dequeue();
        }
    }

    public override void Layout()
    {
        MainOverlay();
    }

    private int memoryGraphHeight = 100;
    private int fpsGraphHeight = 100;

    private bool showGameWindow = true;
    private bool showDebugWindow = true;
    private bool showFpsOverlay = true;
    private bool showDemoWindow = false;
    private bool showLogWindow = true;

    private void MainOverlay()
    {
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize |
                                             ImGuiWindowFlags.NoFocusOnAppearing |
                                             ImGuiWindowFlags.NoNav |
                                             ImGuiWindowFlags.NoDocking;
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("Overlay", windowFlags))
        {
            ImGui.Checkbox("Game window", ref showGameWindow);
            ImGui.Checkbox("Debug window", ref showDebugWindow);
            ImGui.Checkbox("FPS overlay", ref showFpsOverlay);
            ImGui.Checkbox("Demo window", ref showDemoWindow);
            ImGui.Checkbox("Log window", ref showLogWindow);
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();

        if (showDebugWindow) DebugWindow(ref showDebugWindow);
        if (showFpsOverlay) FpsOverlay(ref showFpsOverlay);
        if (showGameWindow) GameWindow(ref showGameWindow);
        if (showDemoWindow) ImGui.ShowDemoWindow(ref showDemoWindow);
        if (showLogWindow) LogWindow(ref showLogWindow);
    }

    private void DebugWindow(ref bool open)
    {
        if (ImGui.Begin("Debug Window", ref open))
        {
            ImGui.Text($"fps: {Performance.GetMonitor(Performance.Monitor.TimeFps)}");

            if (ImGui.CollapsingHeader("FPS"))
            {
                ImGui.Columns(3);
                var vsync = OS.VsyncEnabled;
                ImGui.Checkbox("Enable VSync", ref vsync);
                OS.VsyncEnabled = vsync;

                ImGui.NextColumn();

                var fullscreen = OS.WindowFullscreen;
                ImGui.Checkbox("Fullscreen", ref fullscreen);
                OS.WindowFullscreen = fullscreen;

                ImGui.NextColumn();

                var monitors = new System.Collections.Generic.List<string>();
                for (var i = 0; i < OS.GetScreenCount(); i++)
                {
                    monitors.Add((i + 1) + "");
                }

                var currentScreen = OS.CurrentScreen;
                ImGui.Combo("Screen", ref currentScreen, monitors.ToArray<string>(), monitors.Count);

                if (currentScreen != OS.CurrentScreen)
                {
                    if (OS.WindowFullscreen)
                    {
                        OS.WindowFullscreen = false;
                        OS.CurrentScreen = currentScreen;
                        OS.WindowFullscreen = true;
                    }
                    else
                    {
                        OS.CurrentScreen = currentScreen;
                    }
                }

                ImGui.Columns(1);

                var targetFps = Engine.TargetFps;
                ImGui.SliderInt("Target fps", ref targetFps, 0, 1000);
                Engine.TargetFps = targetFps;

                var targetPhysicsFps = Engine.IterationsPerSecond;
                ImGui.SliderInt("Target physics fps", ref targetPhysicsFps, 1, 60);
                Engine.IterationsPerSecond = targetPhysicsFps;
            }

            if (ImGui.CollapsingHeader("Frame Times"))
            {
                ImGui.SliderInt("Frame Time Graph Height", ref fpsGraphHeight, 1, 300);
                ImGui.SliderInt("Frame Time History Size", ref threshold, 500, 3000);

                var frameArray = processDeltas!.ToArray();
                var physicsFrameArray = physicsProcessDeltas!.ToArray();

                unsafe
                {
                    fixed (float* first = &frameArray[0])
                    {
                        ImGui.Text("Frame times");
                        ImGui.PlotLines("", ref first[0], frameArray.Length, 0, "", 0, frameArray.Max(),
                            new Vector2(0, fpsGraphHeight));
                    }

                    fixed (float* first = &physicsFrameArray[0])
                    {
                        ImGui.Text("Physics frame times");
                        ImGui.PlotLines("", ref first[0], physicsFrameArray.Length, 0, "", 0, physicsFrameArray.Max(),
                            new Vector2(0, fpsGraphHeight));
                    }
                }
            }

            if (ImGui.CollapsingHeader("Memory Usage"))
            {
                var maxStaticMemory = (int) Performance.GetMonitor(Performance.Monitor.MemoryStaticMax);
                var maxDynamicMemory = (int) Performance.GetMonitor(Performance.Monitor.MemoryDynamicMax);

                var staticMemoryArray = staticMemory!.ToArray();
                var dynamicMemoryArray = dynamicMemory!.ToArray();

                ImGui.SliderInt("Memory Graph Height", ref memoryGraphHeight, 1, 300);
                ImGui.SliderInt("Memory History Size", ref memoryThreshold, 250, 1000);
                unsafe
                {
                    fixed (float* first = &staticMemoryArray[0])
                    {
                        ImGui.Text("Static Memory (bytes)");
                        ImGui.PlotLines($"Max: {maxStaticMemory}", ref first[0], staticMemoryArray.Length, 0, "", 0,
                            staticMemoryArray.Max(), new Vector2(0, memoryGraphHeight));
                    }

                    fixed (float* first = &dynamicMemoryArray[0])
                    {
                        ImGui.Text("Dynamic Memory (bytes)");
                        ImGui.PlotLines($"Max: {maxDynamicMemory}", ref first[0], dynamicMemoryArray.Length, 0, "", 0,
                            dynamicMemoryArray.Max(), new Vector2(0, memoryGraphHeight));
                    }
                }
            }

            if (ImGui.CollapsingHeader("Watches"))
            {
                var mousePosition = Extensions.GodotVec2ToSystemVec2(GetViewport().GetMousePosition());
                var gameViewportMousePosition = Extensions.GodotVec2ToSystemVec2(gameViewport!.GetMousePosition());
                var gameMousePosition = Extensions.GodotVec2ToSystemVec2(
                    Extensions.SystemVec2ToGodotVec2(gameViewportMousePosition) - gameWindowPosition);
                ImGui.Text(string.Format("Mouse: {0}, {1}", mousePosition.X, mousePosition.Y));
                ImGui.Text(string.Format("Game Viewport Mouse: {0}, {1}", gameViewportMousePosition.X,
                    gameViewportMousePosition.Y));
                ImGui.Text(string.Format("Game Window Position: {0}, {1}", gameWindowPosition.x, gameWindowPosition.y));
                ImGui.Text(string.Format("Game Mouse: {0}, {1}", gameMousePosition.X, gameMousePosition.Y));
                ImGui.Text(string.Format("Game Viewport: Pos({0}, {1}) Size({2}, {3})",
                    gameViewportRect.Position.x, gameViewportRect.Position.y,
                    gameViewportRect.Size.x, gameViewportRect.Size.y));
                ImGui.Text(string.Format("MouseInsideGame: {0}", mouseInsideGame));
            }

            if (ImGui.CollapsingHeader("Scene Graph"))
            {
                ShowSceneGraph(GetTree().Root);
            }

            if (ImGui.CollapsingHeader("Assertions"))
            {
                if (ImGui.Button("Trigger Equals Assertion"))
                    Assert.Equals(1, 2, "Objects don't match");
                if (ImGui.Button("Trigger True Assertion"))
                    Assert.True(false, "False was found");
                if (ImGui.Button("Trigger False Assertion"))
                    Assert.False(true, "True was found");
                if (ImGui.Button("Trigger NotNull Assertion"))
                    Assert.NotNull(null, "Null was found");
                if (ImGui.Button("Trigger Null Assertion"))
                    Assert.Null(1, "Not null was found");
                if (ImGui.Button("Trigger LessThan Assertion"))
                    Assert.LessThan(1, 1);
                if (ImGui.Button("Trigger LessThanEquals Assertion"))
                    Assert.LessThanEquals(1, 0);
                if (ImGui.Button("Trigger GreaterThan Assertion"))
                    Assert.GreaterThan(1, 1);
                if (ImGui.Button("Trigger GreaterThanEquals Assertion"))
                    Assert.GreaterThanEquals(1, 2);
            }
        }

        ImGui.End();
    }

    private static void ShowSceneGraph(Node root)
    {
        var nodeText = root.Name;
        if (!root.GetClass().Equals(root.Name))
        {
            nodeText = $"{root.GetClass()} ({root.Name})";
        }

        if (root.GetChildren().Count == 0)
        {
            ImGui.TreeNodeEx(nodeText, ImGuiTreeNodeFlags.Leaf);
            ImGui.TreePop();
        }
        else if (ImGui.TreeNode(nodeText))
        {
            foreach (var childObj in root.GetChildren())
            {
                var child = (Node) childObj;
                ShowSceneGraph(child);
            }

            ImGui.TreePop();
        }
    }

    private void FpsOverlay(ref bool open)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                             ImGuiWindowFlags.AlwaysAutoResize |
                                             ImGuiWindowFlags.NoFocusOnAppearing |
                                             ImGuiWindowFlags.NoNav |
                                             ImGuiWindowFlags.NoDocking;
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("Fps Overlay", ref open, windowFlags))
        {
            ImGui.Text($"fps: {Performance.GetMonitor(Performance.Monitor.TimeFps)}");
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
    }

    private void GameWindow(ref bool open)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
        if (ImGui.Begin("Game Window", ref open))
        {
            var imguiTabSize = ImGui.GetWindowSize() - ImGui.GetContentRegionAvail();
            gameWindowPosition = Extensions.SystemVec2ToGodotVec2(ImGui.GetWindowPos() + imguiTabSize);
            gameContainer!.RectPosition = gameWindowPosition;

            var windowSize = Extensions.SystemVec2ToGodotVec2(ImGui.GetContentRegionAvail());
            // gameViewportContainer!.SetSize(windowSize);
            gameViewport!.Size = windowSize;

            var texture = gameViewport!.GetTexture();
            var size = Extensions.GodotVec2ToSystemVec2(texture.GetSize());
            var id = ImGuiGD.BindTexture(texture);

            ImGui.Image(id, size);
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
    }

    private struct ImGuiLogData
    {
        public delegate void LogFunction(string s);

        public ImGuiLog.LogType LogLevel;
        public Vector4 Colour;
        public LogFunction Function;
    };


    private bool showColours = false;
    private bool showLogTesters = false;
    private bool followText = true;
    private bool wordWrap = false;
    private Vector4 scrollbarColour = Extensions.ColorToFloat(new Vector4(76, 208, 183, 255));

    private void LogWindow(ref bool open)
    {
        ImGui.Begin("Log", ref open);

        if (ImGui.Button("Clear"))
        {
            ImGuiLog.Clear();
        }


        ImGui.SameLine();
        ImGui.Checkbox("Show Colours", ref showColours);
        ImGui.SameLine();
        ImGui.Checkbox("Show Log testers", ref showLogTesters);
        ImGui.SameLine();
        ImGui.Checkbox("Word wrap", ref wordWrap);

        if (showColours)
        {
            for (var i = 0; i < imguiLogData!.Count; i++)
            {
                var logData = imguiLogData[i];
                ImGui.ColorEdit4(logData.LogLevel + "", ref logData.Colour);
                imguiLogData[i] = logData;
            }

            ImGui.ColorEdit4("Scrollbar Colour", ref scrollbarColour);
        }

        if (showLogTesters)
        {
            for (var i = 0; i < imguiLogData!.Count; i++)
            {
                var logData = imguiLogData[i];
                if (i > 0)
                    ImGui.SameLine();
                if (ImGui.Button($"Add {logData.LogLevel} text"))
                {
                    logData.Function($"{logData.LogLevel} text");
                    ImGui.SameLine();
                }
            }
        }

        ImGui.Separator();
        var oldFollowText = followText;
        if (oldFollowText)
        {
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, scrollbarColour);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, scrollbarColour);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, scrollbarColour);
        }

        var windowFlags = wordWrap ? ImGuiWindowFlags.None : ImGuiWindowFlags.HorizontalScrollbar;
        ImGui.BeginChild("Log", Vector2.Zero, false, windowFlags);

        followText = Math.Abs(ImGui.GetScrollY() - ImGui.GetScrollMaxY()) < 0.001;


        foreach (var logData in ImGuiLog.GetEntries())
        {
            var logTypeEntry = imguiLogData!.Find(o => o.LogLevel == logData.Type);
            ImGui.PushStyleColor(ImGuiCol.Text, logTypeEntry.Colour);
            var logText = string.Format("[{0}]: {1}", logData.Type, logData.Message);
            if (wordWrap)
            {
                ImGui.TextWrapped(logText);
            }
            else
            {
                ImGui.Text(logText);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted($"{logData.TimeStamp}");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            ImGui.PopStyleColor();
        }


        if (followText)
            ImGui.SetScrollHereY(0);

        ImGui.EndChild();
        if (oldFollowText)
        {
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
        }

        ImGui.End();
    }
}