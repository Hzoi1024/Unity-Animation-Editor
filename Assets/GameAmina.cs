using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAnimaPara
{
    public readonly static float frameTime = 0.33f;
}

public class GameAmina : MonoBehaviour
{
    public readonly static float frameTime = 0.33f;

    List<AminaProcess> processes;

    int processesPointer;

    float DeltaTime;

    // Start is called before the first frame update
    void Start()
    {
        processesPointer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        DeltaTime += Time.deltaTime;

        if(DeltaTime> GameAnimaPara.frameTime)
        {
            //播放下一帧动画
            if (processesPointer != 0)
            {
                Debug.Log("processesPointer到了下一帧还没播放完动画");
                while (processesPointer < processes.Count)
                {
                    processes[processesPointer].AminaPlayNext();
                    ++processesPointer;
                }
                processesPointer = 0;
            }

            int deltaPointer = 0;
            while (processesPointer < processes.Count)
            {
                processes[processesPointer].AminaPlayNext();
                ++processesPointer;
                ++deltaPointer;
                if (deltaPointer > 5) break;
            }

            if(processesPointer>= processes.Count)
            {
                processesPointer = 0;
            }

            DeltaTime -= GameAnimaPara.frameTime;
        }
        else if (processesPointer != 0)
        {
            int deltaPointer = 0;
            while (processesPointer < processes.Count)
            {
                processes[processesPointer].AminaPlayNext();
                ++processesPointer;
                ++deltaPointer;
                if (deltaPointer > 5) break;
            }

            if (processesPointer >= processes.Count)
            {
                processesPointer = 0;
            }
        }


    }
}
