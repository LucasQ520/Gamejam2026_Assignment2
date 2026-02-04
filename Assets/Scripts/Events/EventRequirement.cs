using System;
using UnityEngine;

public enum RequirementType
{
    SingleItem,
    TwoHandsExact,
    Sequence,
    MaskRequired,
    
    UseMainHandItem,
    UseMainHandItemWhileMasked
}

[Serializable]
public class EventRequirement
{
    public RequirementType type;


    public ItemId singleItem;

  
    public ItemId leftItem;
    public ItemId rightItem;


    public ItemId[] sequence;
    
    public MaskController.MaskType requiredMask;

    public int requiredDiceNumber;
}