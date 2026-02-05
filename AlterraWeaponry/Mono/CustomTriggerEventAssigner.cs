#if BZ
namespace VELD.AlterraWeaponry.Mono;

public class CustomTriggerStoryGoalAssigner : MonoBehaviour
{
    public StoryGoal storyGoal { get; private set; }
    public string id { get; private set; }

    public Action action { get; private set; }

    public CustomTriggerStoryGoalAssigner(StoryGoal storyGoal, string id, Action action = null)
    {
        this.storyGoal = storyGoal;
        this.id = id;
        this.action = action;
    }

    private void Start()
    { 
        bool flag = this.GetComponentInParent<BoxCollider>();
        if(!flag)
        {
            throw new Exception($"The CustomTriggerEventAssigner monobehaviour of parent CustomEventTrigger has no Collider.");
        }

        bool flag2 = this.storyGoal != null;
        if(!flag2)
        {
            throw new Exception($"The StoryGoal of the CustomTriggerEventAssigner is undefined.");
        }
    }

    private void OnTriggerEnter(Collision collision)
    {
        bool flag = this.storyGoal != null;
        if(!flag)
        {
            Main.logger.LogWarning($"TriggerBox {this.id} has no StoryGoal to play.");
            return;
        }

        this.storyGoal.Trigger();

        bool flag2 = this.action != null;
        if(flag2) {
            this.action.Invoke();
        }
    }

}
#endif