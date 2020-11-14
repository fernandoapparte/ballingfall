
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSO : MonoBehaviour
{
    public GameObject myPrefab; //Aqui están las partículas
    private GameObject go; //Este es un gameobject cualquiera que le añado ESTE script , el TestSO
    private ParticleSystem ps; //Lo uso para demostrar que accedo correctamente (falta control de errores)

    void Start()
    {
        go = Instantiate(myPrefab, Camera.main.transform);
        go.SetActive(true);
        ps = go.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //Important: 
        //0 - I don't know if it is normal, but Instantiate put the object in the scene but deactived. Is up to the programmer to activate it to make it live.
        //1 - When the Particle System is setActive it will start right away (no need to use play)
        //2 - Stop doesn't stop inmmediately. It will take a while , depending of the prefab.
        //3 - A CAMERA NEEDS TO BE ACTIVE, since I'm attaching  a particle system to the camera. 
        //5 - There is no need to attach de Particle System to the camera. It is only to put the ps exactly in the same place that it was in the project
        //6 - Very important: when Instantiate, the prefab will have the same coordinates,rotation and scale that it was saved as prefab.
        //That means an important data saving (I don't need to put extra data in scriptable objects).

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ps.isPlaying)
            {
                ps.Stop();
                return;
            }
            ps.Play();
        }
    }
}
        /*
        public Material MaterialToChange;
        public float value = 2.0f;
        void Start () {

        }

        // Update is called once per frame
        void Update () {
            //Test modify shaders on runtime. Tests were ok.
            if(Input.GetKeyDown(KeyCode.Space))
            {
                value -= .1f;
                MaterialToChange.SetFloat("_SkyIntensity", value);
            }

        s}
    */

    
