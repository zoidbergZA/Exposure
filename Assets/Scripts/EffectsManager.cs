using UnityEngine;
using System.Collections;

public class EffectsManager : MonoBehaviour
{
    //prefabs
    [SerializeField] private int bufferSize = 10;
    [SerializeField] private SpecialEffect[] effectPrefabs;

    private RotatingBuffer[] buffers;

    void Awake()
    {
        Initialize();
    }

    public void Play(int effectPrefab, Vector3 position, Quaternion rotation)
    {
        buffers[effectPrefab].Play(position, rotation);
    }

    private void Initialize()
    {
        buffers = new RotatingBuffer[effectPrefabs.Length];

        for (int i = 0; i < buffers.Length; i++)
        {
            buffers[i] = new RotatingBuffer(bufferSize);

            for (int j = 0; j < buffers[i].effects.Length; j++)
            {
                buffers[i].effects[j] = Instantiate(effectPrefabs[i], Vector3.zero, Quaternion.identity) as SpecialEffect;
                buffers[i].effects[j].transform.SetParent(transform);
                buffers[i].effects[j].Reset();
            }
        }
    }
}

public class RotatingBuffer
{
    public SpecialEffect[] effects;
    private int head;

    public RotatingBuffer(int bufferSize)
    {
        effects = new SpecialEffect[bufferSize];
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        effects[head].Play(position, rotation);
        
        //move head        
        head++;

        if (head >= effects.Length)
            head = 0;
    }
}
