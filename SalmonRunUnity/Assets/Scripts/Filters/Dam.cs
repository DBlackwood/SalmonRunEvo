﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A dam restricts fish access to upstream areas
 */
public class Dam : FilterBase, IDragAndDropObject
{
    // box where fish will be dropped off if the successfully pass the dam
    private BoxCollider dropOffBox;

    /**
     * Start is called before the first frame update
     */
    void Start()
    {
        
    }

    /** 
     * Update is called once per frame
     */
    void Update()
    {
        
    }

    #region Dam Operation

    /**
     * Activate the dam
     * 
     * @param dropOffBox BoxCollider the box where succesfully passing fish will be dropped off
     */
    public void Activate(BoxCollider dropOffBox)
    {
        this.dropOffBox = dropOffBox;
        active = true;
    }

    #endregion

    #region IDragAndDropObject Implementation

    /**
     * Place the dam onto the game map
     */
    public void Place(RaycastHit primaryHitInfo, List<RaycastHit> secondaryHitInfo)
    {
        // can only place if we are over a dam placement location
        DamPlacementLocation placementLocation = primaryHitInfo.collider.gameObject.GetComponent<DamPlacementLocation>();
        if (placementLocation != null)
        {
            placementLocation.AttachDam(this);
        }
    }

    /**
     * Figure out if we can place the dam at the location of a given raycast
     */
    public bool PlacementValid(RaycastHit primaryHitInfo, List<RaycastHit> secondaryHitInfo)
    {
        // must have hit something
        if (primaryHitInfo.collider)
        {
            DamPlacementLocation placementLocation = primaryHitInfo.collider.gameObject.GetComponent<DamPlacementLocation>();

            // thing we hit must be a dam placement location
            if (placementLocation != null)
            {
                // only return true if the placement location is not already in use
                return !placementLocation.inUse;
            }
        }

        return false;
    }

    #endregion

    #region Base Class (FilterBase) Implementation

    /**
     * Apply the effect of the dam
     * 
     * Fish will attempt to cross the dam and may be able to pass or "get stuck" and die
     */
    protected override void ApplyFilterEffect(Fish fish)
    {
        // only let it through if it hasn't been flagged as stuck
        if (!fish.IsStuck())
        {
            // chance between fish getting past the dam and being caught/getting stuck
            // TODO: make this based on fish traits rather than 50/50 chance
            if (Random.Range(0f, 1f) <= 0.1f)
            {
                fish.transform.position = GetRandomDropOff(fish.transform.position.z);
            }
            // if it didn't make it, make it permanently stuck (so it can't try repeated times)
            else
            {
                fish.SetStuck(true);
            }
        }
        
    }

    /**
     * Get a random point within the drop off collider
     * 
     * @param z float The z value of the point -- don't want to change the object's z so just pass it in
     */
    private Vector3 GetRandomDropOff(float z)
    {
        return new Vector3(
            Random.Range(dropOffBox.bounds.min.x, dropOffBox.bounds.max.x),
            Random.Range(dropOffBox.bounds.min.y, dropOffBox.bounds.max.y),
            z
        );
    }

    #endregion
}
