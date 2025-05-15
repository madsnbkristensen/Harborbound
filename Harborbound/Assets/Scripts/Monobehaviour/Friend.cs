using UnityEngine;

public class Friend : Humanoid
{
    public enum state { PATROLLING, CHASING, FROZEN }
    public enum type { NPC, MERCHANT, QUEST_GIVER }
    [Header("Friend Properties")]
    [SerializeField]
    public state friendState = state.PATROLLING;
    [SerializeField]
    public type friendType = type.NPC;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        if (humanoidName == null)
            humanoidName = "Friend";
    }

    // Update is called once per frame
    void Update()
    {
        // Friend AI logic goes here
        // For example, assist player or provide items
    }

    // dialogue system
    public void StartDialogue()
    {
        // Start dialogue with player
        Debug.Log($"{humanoidName} is starting a dialogue.");
    }

    public void EndDialogue()
    {
        // Change friend state back to what it was before dialogue
        friendState = state.PATROLLING; // Or restore previous state

        Debug.Log($"{humanoidName} has ended dialogue.");
    }
}
