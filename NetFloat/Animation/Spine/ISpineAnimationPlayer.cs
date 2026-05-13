namespace NetFloat.Animation.Spine;

public interface ISpineAnimationPlayer
{
    void Play(SpineActionName actionName, SpineActionRequest request);
}
