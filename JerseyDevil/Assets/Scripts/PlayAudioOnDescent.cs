using UnityEngine;

public class PlayAudioOnStairDescent : MonoBehaviour
{
    public AudioClip fallAudioClip;
    private AudioSource audioSource;
    public float fallThreshold = 2.5f;
    //public GameObject step1;
    //public GameObject step2;
    bool hasPlayed;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        
    }

    void Update()
    {
        if (transform.position.y < fallThreshold && !hasPlayed)
        {
            Debug.Log("here");
            audioSource.Stop();
            audioSource.clip = fallAudioClip;
            audioSource.Play();
            hasPlayed = true; 
            Invoke(methodName: "AfterAudio", audioSource.clip.length);
        }
    }
}
