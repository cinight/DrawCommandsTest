using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTransforms
{
    public static Vector3[] GenerateObjPos(int count, Vector3 wpos, float spacing)
    {
        Vector3[] positions = new Vector3[count];

        int gridCount = Mathf.FloorToInt(Mathf.Pow(count, 1f / 3f));
        int xId = 0;
        int yId = 0;
        int zId = 0;

        for(int i=0; i<count; i++)
        {
            Vector3 pos = new Vector3(xId*spacing, yId*spacing, zId*spacing);
            positions[i] = pos + wpos;
            
            xId++;
            if(xId>=gridCount)
            {
                xId = 0;
                yId ++;
            }
            if(yId>=gridCount)
            {
                yId = 0;
                zId ++;
            }
        }

        return positions;
    }

    public static Quaternion[] GenerateObjRot(int count)
    {
        Quaternion[] rotations = new Quaternion[count];

        for(int i=0; i<count; i++)
        {
            //Rotation
            Vector3 rot;
            rot.x = UnityEngine.Random.Range(0, 360);
            rot.y = UnityEngine.Random.Range(0, 360);
            rot.z = UnityEngine.Random.Range(0, 360);
            rotations[i] = Quaternion.Euler(rot.x, rot.y, rot.z);
        }

        return rotations;
    }

    public static Vector3[] GenerateObjScale(int count, float min, float max)
    {
        Vector3[] scales = new Vector3[count];

        for(int i=0; i<count; i++)
        {
            //Scale
            Vector3 sca;
            sca.x = UnityEngine.Random.Range(min, max);
            sca.y = UnityEngine.Random.Range(min, max);
            sca.z = UnityEngine.Random.Range(min, max);
            scales[i] = new Vector3(sca.x, sca.y, sca.z);
        }

        return scales;
    }
}
