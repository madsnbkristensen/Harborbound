using UnityEngine;

public class Enemy : Humanoid
{
    public enum state { PATROLLING, ATTACKING, CHASING, SEARCHING }
    public enum type { PIRATE, SHARK }

    [Header("Enemy Properties")]
    public int attackDamage = 10;
    public float attackRange = 2f;
    public float attackSpeed = 2f;

    [SerializeField]
    public state enemyState = state.PATROLLING;
    [SerializeField]
    public type enemyType = type.PIRATE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        if (humanoidName == null)
            humanoidName = "Enemy";
    }

    // Update is called once per frame
    void Update()
    {
        // Enemy AI logic goes here
        // For example, move towards player or attack if in range
    }
    protected void handlePatrolState()
    {

    }
    protected void handleChaseState()
    {

    }
    protected void handleAttackState()
    {

    }
    protected void handleSearchState()
    {

    }
}
