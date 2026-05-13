using System.Windows;
using NetFloat.Animation.Spine;

namespace NetFloat.Windows;

public partial class FaceWindow : Window
{
    private readonly ISpineAnimationPlayer _animationPlayer;

    public FaceWindow()
    {
        InitializeComponent();
        _animationPlayer = new WpfSpineActionPlayer(this, OpenCharacterImage, ClosedCharacterImage, EffectCanvas);
    }

    public void PlayEatFile(Window ownerWindow, Point dropPoint, Vector dragDirection)
    {
        _animationPlayer.Play(
            SpineActionName.EatFile,
            new SpineActionRequest(
                dropPoint,
                dragDirection,
                () =>
                {
                    Close();
                    ownerWindow.Show();
                }));
    }
}
