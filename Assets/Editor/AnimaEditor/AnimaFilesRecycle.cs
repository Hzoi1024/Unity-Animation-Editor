using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class AnimaFilesRecycle
{
    static readonly string RECYCLEFILES = "Assets/Editor/AnimaEditor/RecycleFiles/";

    public static void DeleteFrames(string _path)
    {
        string _name = "[frames]"+ _path.Substring(_path.LastIndexOf(@"/") + 1);
        

        if (File.Exists(RECYCLEFILES + _name))
        {
            int k = 1;

            string[] _s = _name.Split('.');

            if (_s.Length == 2)
            {
                while (k < 1000)
                {
                    string _newName = _s[0] + "(" + k.ToString() + ")" + "." + _s[1];

                    if (!File.Exists(RECYCLEFILES + _newName))
                    {
                        _name = _newName;
                        break;
                    }

                    k++;
                }
            }
            else
            {
                while (k < 1000)
                {
                    string _newName = _name + "(" + k.ToString() + ")";

                    if (!File.Exists(RECYCLEFILES + _newName))
                    {
                        _name = _newName;
                        break;
                    }

                    k++;
                }
            }


            
        }

        AssetDatabase.MoveAsset(_path, RECYCLEFILES + _name);

    }



}
