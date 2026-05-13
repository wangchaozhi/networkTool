using System;
using System.Windows;

namespace NetFloat.Animation.Spine;

public sealed record SpineActionRequest(
    Point DropPoint,
    Vector DragDirection,
    Action? Completed = null);
