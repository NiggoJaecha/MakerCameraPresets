using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MakerCameraPresets
{
    [Serializable]
    public class PluginData
    {
        [SerializeField]
        public List<CameraData> cameras { get; set; }
    }

    [Serializable]
    public class CameraData
    {
        [SerializeField]
        public float[] position { get; set; }
        [SerializeField]
        public float[] angle { get; set; }
        [SerializeField]
        public float[] direction { get; set; }
        [SerializeField]
        public float fov { get; set; }

        public CameraData()
        {
            position = new float[3];
            angle = new float[3];
            direction = new float[3];
            fov = 0f;
        }
        public CameraData(Vector3 positionVector, Vector3 angleVector, Vector3 directionVector, float fovValue)
        {
            position = new float[3];
            setPosition(positionVector);
            angle = new float[3];
            setAngle(angleVector);
            direction = new float[3];
            setDirection(directionVector);
            fov = fovValue;
        }

        public void setPosition(Vector3 vector)
        {
            position[0] = vector.x;
            position[1] = vector.y;
            position[2] = vector.z;
        }
        public Vector3 getPosition()
        {
            return new Vector3(position[0], position[1], position[2]);
        }

        public void setAngle(Vector3 vector)
        {
            angle[0] = vector.x;
            angle[1] = vector.y;
            angle[2] = vector.z;
        }
        public Vector3 getAngle()
        {
            return new Vector3(angle[0], angle[1], angle[2]);
        }

        public void setDirection(Vector3 vector)
        {
            direction[0] = vector.x;
            direction[1] = vector.y;
            direction[2] = vector.z;
        }
        public Vector3 getDirection()
        {
            return new Vector3(direction[0], direction[1], direction[2]);
        }
    }
}
