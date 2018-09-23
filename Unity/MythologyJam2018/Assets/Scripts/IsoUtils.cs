using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUtils : MonoBehaviour
{
    private static IsoUtils instance = null;
    private static float zScreenAdjustment = 1 - (35.625f / 90f);

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out IsoUtils isoUtils)
    {
        isoUtils = instance;
        return (isoUtils != null);
    }

    public static Vector3 TransformVectorToScreenSpace(Vector3 input)
    {
        IsoUtils utils;
        if (TryGetInstance(out utils))
        {
            Vector3 transformedDirection = utils.transform.TransformVector(input);
            return transformedDirection * (1 + Mathf.Abs(Vector3.Dot(transformedDirection, utils.transform.forward)) * zScreenAdjustment);
        }

        return Vector3.zero;
    }
}
