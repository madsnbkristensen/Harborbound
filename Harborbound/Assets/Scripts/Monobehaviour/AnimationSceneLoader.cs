using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "NextScene";
    private Animator animator;
    private bool hasLoaded = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!hasLoaded)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
            {
                hasLoaded = true;
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
