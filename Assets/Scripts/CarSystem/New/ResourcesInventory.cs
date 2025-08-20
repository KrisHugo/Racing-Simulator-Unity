using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceInventory : MonoBehaviour
{
    public Dictionary<string, int> resources = new Dictionary<string, int>();

    

    public bool HasResources(string resource, int cost)
    {
        if (resources.ContainsKey(resource) && resources[resource] >= cost)
        {
            return true;
        }
        return false;
    }

    public void UseResources(string resource, int cost)
    {
        if (HasResources(resource, cost))
        {
            resources[resource] = Mathf.Max(resources[resource] - cost, 0);
        }
    }
}
