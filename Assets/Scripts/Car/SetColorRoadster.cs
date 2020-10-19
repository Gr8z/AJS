using UnityEngine;
using System.Collections;

//To be able to give the Roadster a random color
public class SetColorRoadster : MonoBehaviour
{
  public MeshRenderer carBody;


  public void SetColor(Material carMaterial)
  {
    Material[] bodyMaterials = carBody.materials;

    //It's the first in the array that's the body paint
    bodyMaterials[0] = carMaterial;

    carBody.materials = bodyMaterials;
  }
}
