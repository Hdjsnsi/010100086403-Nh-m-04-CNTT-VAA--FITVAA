using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SoldierType
{
    public enum Type
    {
        Aircaft,
        Tank,
        Regular
    }
    public static class Detecting 
    {
        static GameObject[] allEnemy;
        public static Vector3 DetectEnemy(Vector3 position,string tag)
        {
            allEnemy = GameObject.FindGameObjectsWithTag(tag);
            if(allEnemy.Length <= 0) return Vector3.zero;
            GameObject objectEnemy = allEnemy.OrderBy(a => (a.transform.position - position).sqrMagnitude).FirstOrDefault();
            if(objectEnemy == null) return Vector3.zero;
            return objectEnemy.transform.position;
        }
        
    }
}
