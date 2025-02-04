using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

/// <summary>
/// A script to dynamically switch between two NN models at runtime.
/// This script changes the model and updates the GameObject's appearance 
/// (color) to reflect the currently active model.
/// </summary>
public class ModelSwitcher : MonoBehaviour
{
    /// <summary>
    /// The first ONNX model to be used by the agent.
    /// </summary>
    public NNModel model1;

    /// <summary>
    /// The second ONNX model to be used by the agent.
    /// </summary>
    public NNModel model2;

    /// <summary>
    /// The color applied to the sprite when using the first model.
    /// </summary>
    public Color colorForModel1 = Color.green;

    /// <summary>
    /// The color applied to the sprite when using the second model.
    /// </summary>
    public Color colorForModel2 = Color.red;

    private BehaviorParameters behaviorParameters; ///< Reference to the BehaviorParameters component.
    private SpriteRenderer spriteRenderer; ///< Reference to the SpriteRenderer component.
    private bool usingModel1 = true; ///< Tracks whether the first model is currently active.

    /// <summary>
    /// Initializes the script by setting the first model and color as active.
    /// </summary>
    void Start()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Use the first model and color by default.
        behaviorParameters.Model = model1;
        spriteRenderer.color = colorForModel1;
    }

    /// <summary>
    /// Checks for input to switch the active model and updates the agent's appearance accordingly.
    /// </summary>
    void Update()
    {
        // Switch models when the "M" key is pressed.
        if (Input.GetKeyDown(KeyCode.M))
        {
            usingModel1 = !usingModel1;

            if (usingModel1)
            {
                behaviorParameters.Model = model1;
                spriteRenderer.color = colorForModel1;
            }
            else
            {
                behaviorParameters.Model = model2;
                spriteRenderer.color = colorForModel2;
            }

            Debug.Log("Model switched to: " + behaviorParameters.Model.name);
        }
    }
}
