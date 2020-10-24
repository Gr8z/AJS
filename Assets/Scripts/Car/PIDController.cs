using UnityEngine;
using System.Collections;

public class PIDController : MonoBehaviour
{
  float CTE_old = 0f;
  float CTE_sum = 0f;

  public float GetSteerFactorFromPIDController(float CTE)
  {
    float tau_P = 50f;
    float tau_I = 0.05f;
    float tau_D = 30f;


    //The steering factor
    float alpha = 0f;


    //P
    alpha = tau_P * CTE;


    //I
    CTE_sum += Time.fixedDeltaTime * CTE;

    alpha += tau_I * CTE_sum;


    //D
    float d_dt_CTE = (CTE - CTE_old) / Time.fixedDeltaTime;

    alpha += tau_D * d_dt_CTE;

    CTE_old = CTE;

    return alpha;
  }
}
