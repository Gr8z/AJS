using UnityEngine;
using System.Collections;

public class EmergencyLight : MonoBehaviour
{
  //Material, such as red or blue
  Color lightColor;
  // offColor;

  float lightTimer = 0f;

  bool isStandard = true;

  MeshRenderer meshRenderer;


  void Start()
  {
    meshRenderer = GetComponent<MeshRenderer>();

    lightColor = meshRenderer.material.color;

    //offColor = new Material();

    //So both red and blue dont switch at the same time
    lightTimer = Random.Range(0f, 0.2f);
  }



  void Update()
  {
    lightTimer += Time.deltaTime;

    //Change color
    if (lightTimer > 0.2f)
    {
      lightTimer = 0f;

      isStandard = !isStandard;

      if (isStandard)
      {
        meshRenderer.material.color = lightColor;
      }
      else
      {
        meshRenderer.material.color = Color.white;
      }
    }
  }
}
