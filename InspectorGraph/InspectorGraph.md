```cs
using UnityEngine;

public class InspectorGraphExample : MonoBehaviour
{
    [Header("Func vars")]

    [Range(0, 3)]
    public float damping = 0f;

    [Range(1, 1.5f)]
    public float frequency = 1f;

    [Header("Graph")]

    public InspectorGraph value;

    private void OnValidate()
    {
        value.function = func;

        value.skin = graphSkin;
    }

    public InspectorGraph.Skin graphSkin;

    float func(float x)
    {
        // return Mathf.Abs(Mathf.Sin(x + damping) * frequency);

        float k = 2.55015f, e = 2.71828f, m = frequency, u = damping;

        var d = 1f - k;

        var e_pow = d + (k - x - d) * (m * 1.1f);

        var f1 = Mathf.Pow( ( 1f - x ) , u ) / 2f;

        // https://www.desmos.com/calculator/glvu0t6z61

        return f1 * Mathf.Sin(Mathf.Pow(e, e_pow)) + f1;
    }
}
```
