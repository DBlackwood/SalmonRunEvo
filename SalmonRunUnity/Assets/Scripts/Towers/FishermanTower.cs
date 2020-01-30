﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishermanTower : TowerBase
{
    public Material hitLineMaterial;
    public Material missLineMaterial;
    public Material flashMaterial;

    // how many times the fish will flash in and out to show it is being caught
    public int numFlashesPerCatch;

    // fish that have been caught by this fisherman
    private List<Fish> caughtFish;

    // fish that the catch attempt line is pointing at
    private Fish catchAttemptFish;

    // LineRenderer component used to display a catch or catch attempt
    private LineRenderer catchAttemptLine;

    /**
     * Start is called on object initialization
     */   
    protected override void Awake()
    {
        base.Awake();
    }

    /**
     * Called before the first frame update
     */
    protected override void Start()
    {

        base.Start();

        caughtFish = new List<Fish>();
        catchAttemptLine = GetComponent<LineRenderer>();
        catchAttemptLine.enabled = false;
    }

    /**
     * Update is called once per frame
     */
    private void Update()
    {
        // update fish line position
        if (catchAttemptLine.enabled)
        {
            SetLinePos();
        }
    }

    /**
     * Determine whether a fisherman could be placed at the location specified by a raycast
     * 
     * @param primaryHitInfo RaycastHit The results of the raycast that was done
     * @param secondaryHitInfo List RaycastHit The results of any secondary raycasts that were done
     */
    protected override bool TowerPlacementValid(RaycastHit primaryHitInfo, List<RaycastHit> secondaryHitInfo)
    {
        int correctLayer = LayerMask.NameToLayer(Layers.TERRAIN_LAYER_NAME);

        // for placement to be valid, primary raycast must have hit a gameobject on the Terrain layer
        if (primaryHitInfo.collider && primaryHitInfo.collider.gameObject.layer == correctLayer)
        {
            // secondary raycasts must also hit gameobjects on the Terrain layer at approximately the same z-pos as the primary raycast
            return secondaryHitInfo.TrueForAll((hitInfo) =>
            {
                return hitInfo.collider &&
                       hitInfo.collider.gameObject.layer == correctLayer &&
                       Mathf.Abs(hitInfo.point.z - primaryHitInfo.point.z) < 0.1f;
            });
        }

        // if one of these conditions was not met, return false
        return false;
    }

    /**
     * Position the fisherman at the correct location using the information from a raycast
     * 
     * @param primaryHitInfo RaycastHit The results of the primary raycast that was done
     * @param secondaryHitInfo List RaycastHit The results of any secondary raycasts that were done
     */
    protected override void PlaceTower(RaycastHit primaryHitInfo, List<RaycastHit> secondaryHitInfo)
    {
        transform.position = primaryHitInfo.point;
    }

    /**
     * Apply the effects of the fisherman tower
     */
    protected override void ApplyTowerEffect()
    {
        Collider[] fishColliders = Physics.OverlapSphere(transform.position, GetEffectRadius(), LayerMask.GetMask(Layers.FISH_LAYER_NAME));

        if (fishColliders.Length > 0)
        {
            Fish f = fishColliders[Random.Range(0, fishColliders.Length)].GetComponent<Fish>();

            if (f != null)
            {
                transform.parent.LookAt(f.transform, Vector3.back);

                TryCatchFish(f);
            }
            else
            {
                Debug.LogError("Something on the fish layer does not have a Fish component!");
            }
        }
        
    }

    /**
     * Attempt to catch a fish
     */
    private void TryCatchFish(Fish f)
    {
        StartCoroutine(TryCatchFishCoroutine(f));
    }

    /**
     * Display attempt to catch fish
     */
    private IEnumerator TryCatchFishCoroutine(Fish fish)
    {
        // figure out whether the fish will be caught or not
        bool caught = Random.Range(0f, 1f) >= 0.75f;

        // do setup for catch attempt line visualizer
        catchAttemptFish = fish;

        Destroy(catchAttemptLine.material);
        catchAttemptLine.material = caught ? hitLineMaterial : missLineMaterial;

        catchAttemptLine.enabled = true;

        // handle fish being caught
        if (caught)
        {
            // make the fish flash  for a bit
            SkinnedMeshRenderer fishRenderer = fish.GetComponentInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < numFlashesPerCatch; i++)
            {
                Material oldMaterial = fishRenderer.material;
                fishRenderer.material = flashMaterial;
                yield return new WaitForSeconds((float)timePerApplyEffect / numFlashesPerCatch / 2f);
                Destroy(fishRenderer.material);
                fishRenderer.material = oldMaterial;
                yield return new WaitForSeconds((float)timePerApplyEffect / numFlashesPerCatch / 2f);
            }

            // actually catch the fish
            fish.Catch();
            caughtFish.Add(fish);
        }
        // fish escaped -- just wait for end of action
        else
        {
            yield return new WaitForSeconds(timePerApplyEffect);
        }

        // end the catch attempt line
        catchAttemptLine.enabled = false;
    }

    /**
     * Set line position for a fish
     */
    private void SetLinePos()
    {
        Vector3 startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 fishPos = catchAttemptFish.transform.position;
        fishPos.z = startPos.z;

        catchAttemptLine.SetPositions(new Vector3[]{ startPos, fishPos});
    }
}