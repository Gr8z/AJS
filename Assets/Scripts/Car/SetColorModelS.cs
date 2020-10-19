using UnityEngine;
using System.Collections;

//To be able to give the Model S a random color
public class SetColorModelS : MonoBehaviour
{
  public MeshRenderer carBody;
  public MeshRenderer frunk;
  public MeshRenderer trunk;
  public MeshRenderer doorBack;
  public MeshRenderer doorFront;



  public void SetColor(Material carMaterial)
  {
    //Body
    ChangeColorFromPosInArray(carBody, 0, carMaterial);

    //Frunk
    ChangeColorFromPosInArray(frunk, 0, carMaterial);

    //Trunk
    ChangeColorFromPosInArray(trunk, 0, carMaterial);

    //Door back
    ChangeColorFromPosInArray(doorBack, 0, carMaterial);

    //Door front
    ChangeColorFromPosInArray(doorFront, 0, carMaterial);
  }



  void ChangeColorFromPosInArray(MeshRenderer meshRenderer, int arrayPos, Material carMaterial)
  {
    //Get all materials that belong to this mesh
    Material[] allMaterials = meshRenderer.materials;

    //Change the material we want to change
    allMaterials[arrayPos] = carMaterial;

    meshRenderer.materials = allMaterials;
  }
}
