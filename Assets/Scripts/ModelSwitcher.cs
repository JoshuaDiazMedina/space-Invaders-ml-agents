using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class ModelSwitcher : MonoBehaviour
{
    public NNModel model1; // Primer modelo ONNX
    public NNModel model2; // Segundo modelo ONNX

    public Color colorForModel1 = Color.green; // Color para el modelo 1
    public Color colorForModel2 = Color.red;   // Color para el modelo 2

    private BehaviorParameters behaviorParameters;
    private SpriteRenderer spriteRenderer;
    private bool usingModel1 = true;

    void Start()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicialmente usa el primer modelo y color
        behaviorParameters.Model = model1;
        spriteRenderer.color = colorForModel1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) // Cambia el modelo al presionar "M"
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

            Debug.Log("Modelo cambiado a: " + behaviorParameters.Model.name);
        }
    }
}
