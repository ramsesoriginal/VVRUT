using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slides {
    public class Navigation : MonoBehaviour {

        public string nextScene;
        public string prevScene;
        	
	    // Update is called once per frame
	    void Update () {
		    if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1") || Input.GetAxis("Horizontal") > 0.1f || Input.GetAxis("Vertical") < -0.1f)
            {
                NextScene();
            }
            if (Input.GetButtonDown("Cancel") || Input.GetAxis("Horizontal") < -0.1f || Input.GetAxis("Vertical") > 0.1f)
            {
                PrevScene();
            }
        }

        public void NextScene()
        {
            if (nextScene != null && nextScene.Length > 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
            }
        }

        public void PrevScene()
        {
            if (prevScene != null && prevScene.Length > 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(prevScene);
            }
        }
    }
}