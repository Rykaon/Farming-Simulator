using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts;
using DelaunatorSharp;

public class BuildObject : MonoBehaviour
{
    private PlayerManager playerManager;
    [SerializeField] Animator animator;
    [SerializeField] private Material placingMaterial;
    private Material startMaterial;
    private List<MeshRenderer> meshRenderer;
    private List<SkinnedMeshRenderer> skinnedMeshRenderer;
    private GameObject itemPrefab;
    private void Awake()
    {
        playerManager = PlayerManager.instance;
        meshRenderer = new List<MeshRenderer>();
        skinnedMeshRenderer = new List<SkinnedMeshRenderer>();

        itemPrefab = playerManager.objectPrefab;
        for (int i = 0; i < animator.transform.childCount; i++)
        {
            MeshRenderer childMeshRenderer = animator.transform.GetChild(i).GetComponent<MeshRenderer>();
            SkinnedMeshRenderer childSkinnedMeshRender = animator.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
            if (childMeshRenderer != null)
            {
                meshRenderer.Add(childMeshRenderer);
            }
            else if (childSkinnedMeshRender != null)
            {
                skinnedMeshRenderer.Add(childSkinnedMeshRender);
            }
            else if (animator.transform.GetChild(i).childCount > 0 && animator.transform.GetChild(i).name != "Armature")
            {
                for (int j = 0; j < animator.transform.GetChild(i).childCount; j++)
                {
                    childMeshRenderer = animator.transform.GetChild(i).GetChild(j).GetComponent<MeshRenderer>();
                    if (childMeshRenderer != null)
                    {
                        meshRenderer.Add(childMeshRenderer);
                        if (animator.transform.GetChild(i).GetChild(j).childCount > 0)
                        {
                            for (int k = 0; k < animator.transform.GetChild(i).GetChild(j).childCount; k++)
                            {
                                childMeshRenderer = animator.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<MeshRenderer>();
                                if (childMeshRenderer != null)
                                {
                                    meshRenderer.Add(childMeshRenderer);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (skinnedMeshRenderer.Count > 0)
        {
            startMaterial = skinnedMeshRenderer[0].material;
        }
        else
        {
            startMaterial = meshRenderer[0].material;
        }
    }

    public void Initialize()
    {
        if (skinnedMeshRenderer.Count > 0)
        {
            for (int i = 0; i < skinnedMeshRenderer.Count; i++)
            {
                skinnedMeshRenderer[i].material = placingMaterial;
            }
        }
        else
        {
            for (int i = 0; i < meshRenderer.Count; i++)
            {
                meshRenderer[i].material = placingMaterial;
            }
        }
        
        animator.SetBool("Action", true);
    }

    public void Rotate(float value)
    {
        if (value == 1)
        {
            transform.Rotate(new Vector3(0, 90f, 0));
        }
        else if (value == -1)
        {
            transform.Rotate(new Vector3(0, -90f, 0));
        }
    }

    public virtual void Place()
    {
        if (transform.tag == "Plant")
        {
            transform.gameObject.GetComponent<PlantManager>().CalculateTargetNode(0);
            PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            if (node != null)
            {
                if (!node.isVirtual)
                {
                    Debug.Log(node.tileManager);
                    node.tileManager.plant = transform.gameObject;
                    node.tileManager.plantItem = itemPrefab;
                }
            }
        }
        else if (transform.tag == "Object")
        {

        }
        animator.SetBool("Action", false);
        if (skinnedMeshRenderer.Count > 0)
        {
            for (int i = 0; i < skinnedMeshRenderer.Count; i++)
            {
                skinnedMeshRenderer[i].material = startMaterial;
            }
        }
        else
        {
            for (int i = 0; i < meshRenderer.Count; i++)
            {
                meshRenderer[i].material = startMaterial;
            }
        }
        Destroy(gameObject.GetComponent<BuildObjectDrag>());
        Destroy(this);
    }
}
