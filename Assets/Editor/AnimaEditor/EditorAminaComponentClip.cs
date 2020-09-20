using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EditorAminaComponentClip", menuName = "Custom/EditorAminaComponentClip")]
public class EditorAminaComponentClip:ScriptableObject
{
    public int CompID;

    public int framesCount { get { return frames.Count; } }

    //public List<int> keyNum;
    public List<Key> keys;
    public List<AminaFrame> frames;
    
    public class Key:IComparable<Key>
    {
        public int index;
        public KeyType type;

        public Key(int _index,KeyType _type)
        {
            index = _index;
            type = _type;
        }

        public int CompareTo(Key other)
        {
            if (this.index > other.index) return 1;
            else if (this.index == other.index) return 0;
            else return -1;
        }

    }

    

    public EditorAminaComponentClip()
    {
        //keyNum = new List<int>();
        frames = new List<AminaFrame>();
        keys = new List<Key>();
        

    }

    public void AddKeyFrame(Vector2 _pos, float _angle, Key _newKey)
    {
        bool isAddNewKey = true ;

        for(int i = 0; i < keys.Count; i++)
        {
            if (keys[i].index == _newKey.index)
            {
                keys[i] = _newKey;
                isAddNewKey = false;
                break;
            }
        }

        if (isAddNewKey)
        {
            keys.Add(_newKey);
            keys.Sort();
        }

        switch (_newKey.type)
        {
            case KeyType.LinearKey:
                AddNewKeyLinear(_pos, _angle, _newKey);
                break;
            case KeyType.AccelerateKey:
                AddNewKeyAcceleration(_pos, _angle, _newKey);
                break;
        }

        
    }

    private void AddNewKeyLinear(Vector2 _pos, float _angle, Key _newKey)
    {
        if (keys.Count == 1)
        {
            frames = new List<AminaFrame>();

            for (int i = 0; i <= keys[0].index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames.Add(_af);
            }
            return;
        }


        int k = GetIndexOf(_newKey.index);
        if (k == 0)
        {
            for (int i = 0; i <= _newKey.index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames[i] = _af;
            }

            int nextIndex = keys[k + 1].index;
            AminaFrame _end = frames[nextIndex];
            float _deltaAng = (_end.angle - _angle) / (nextIndex - _newKey.index);

            for (int i = _newKey.index + 1; i < nextIndex; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = Vector2.Lerp(frames[i - 1].pos, _end.pos, 1f / (nextIndex - i + 1));
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));
                _af.angle = frames[i - 1].angle + _deltaAng;
                frames[i] = _af;
            }

        }
        else
        if (k == keys.Count - 1)
        {
            int previousIndex = keys[k - 1].index;
            AminaFrame _end = new AminaFrame();
            _end.pos = new Vector2(_pos.x, _pos.y);
            _end.angle = _angle;
            float _deltaAng = (_angle - frames[previousIndex].angle) / (_newKey.index - previousIndex);

            bool isAdd = false;
            if (_newKey.index > frames.Count - 1) isAdd = true;

            for (int i = previousIndex + 1; i < _newKey.index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = Vector2.Lerp(frames[i - 1].pos, _end.pos, 1f / (_newKey.index - i + 1));
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (_index - i + 1));
                _af.angle = frames[i - 1].angle + (_angle - frames[i - 1].angle) / (_newKey.index - i + 1);
                //_af.angle = frames[i - 1].angle + _deltaAng;

                if (isAdd)
                {
                    frames.Add(_af);
                }
                else
                {
                    frames[i] = _af;
                }
            }

            if (isAdd)
            {
                frames.Add(_end);
            }
            else
            {
                frames[_newKey.index] = _end;
            }
        }
        else
        {
            AminaFrame _key = new AminaFrame();
            _key.pos = new Vector2(_pos.x, _pos.y);
            _key.angle = _angle;

            frames[_newKey.index] = _key;
            InsertFrame(k);/*
            int previousIndex = keys[k - 1].index;
            AminaFrame _key = new AminaFrame();
            _key.pos = new Vector2(_pos.x, _pos.y);
            _key.angle = _angle;

            frames[_newKey.index] = _key;

            float _preDelAngle = (_angle - frames[previousIndex].angle) / (_newKey.index - previousIndex);

            for (int i = previousIndex + 1; i < _newKey.index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = Vector2.Lerp(frames[i - 1].pos, _key.pos, 1f / (_newKey.index - i + 1));
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _key.angle, 1f / (_index - i + 1));
                //_af.angle= (_key.angle- frames[i - 1].angle)/ (_index - i + 1);
                _af.angle = frames[i - 1].angle + _preDelAngle;
                frames[i] = _af;
            }

            int nextIndex = keys[k + 1].index;
            AminaFrame _end = frames[nextIndex];

            float _nextDelAngle = (_end.angle - _angle) / (nextIndex - _newKey.index);

            for (int i = _newKey.index + 1; i < nextIndex; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = Vector2.Lerp(frames[i - 1].pos, _end.pos, 1f / (nextIndex - i + 1));
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));
                //_af.angle= (_end.angle - frames[i - 1].angle) / (nextIndex - i + 1);
                _af.angle = frames[i - 1].angle + _nextDelAngle;
                frames[i] = _af;
            }*/

        }
    }

    public void DeleteKeyFrame(int _keyIndex)
    {
        bool isCotains = false;

        foreach(Key _key in keys)
        {
            if (_key.index == _keyIndex)
            {
                isCotains = true;
                break;
            }
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index");
            return;
        }

        if (keys.Count == 1)
        {
            keys = new List<Key>();
            frames = new List<AminaFrame>();
            return;
        }

        int k = GetIndexOf(_keyIndex);
        if (k == 0)
        {
            AminaFrame _nextFrame = frames[keys[k + 1].index];

            for(int i=0;i< keys[k + 1].index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_nextFrame.pos.x, _nextFrame.pos.y);
                _af.angle = _nextFrame.angle;
                frames[i] = _af;
            }            
        }else 
        if(k == keys.Count - 1)
        {
            int preIndex = keys[k - 1].index;

            for(int i=_keyIndex;i> preIndex; i--)
            {
                frames.RemoveAt(i);
            }
        }
        else
        {
            int preIndex = keys[k - 1].index;

            Key nextKey = keys[k + 1];

            switch (nextKey.type)
            {
                case KeyType.AccelerateKey:
                    AccelerateFillFrame(preIndex, nextKey.index);
                    break;
                case KeyType.LinearKey:
                    LinearFillFrame(preIndex, nextKey.index);
                    break;
                default:
                    Debug.LogError("delete nextkey type not right");
                    break;

            }

        }



        keys.RemoveAt(k);
    }

    public void MoveKey(int _fromIndex,int _deltaIndex)
    {
        /*if (!keyNum.Contains(_fromIndex))
        {
            Debug.Log("can not MoveKey the key,wrong fromIndex ");
            return;
        }*/

        bool isContains = false;

        foreach (Key _key in keys)
        {
            if (_key.index == _fromIndex)
            {
                isContains = true;
                break;
            }
        }

        if (!isContains)
        {
            Debug.Log("can not MoveKey the key,wrong fromIndex ");
            return;
        }


        int _targetIndex = _fromIndex + _deltaIndex;

        if (_targetIndex < 0)
        {
            Debug.Log("can not MoveKey the key,targetIndex <0 ");
            return;
        }

        if (_deltaIndex == 0)
        {
            return;
        }

        int keyIndex = GetIndexOf(_fromIndex);

        Key _moveKey = keys[keyIndex];
        if (keys.Count == 1)
        {
            _moveKey.index = _fromIndex + _deltaIndex;
            if (_deltaIndex < 0)
            {
                for(int i = _fromIndex; i > _targetIndex; i--)
                {
                    frames.RemoveAt(i);
                }
            }
            else
            {
                for (int i = _fromIndex+1; i <= _targetIndex; i++)
                {
                    AminaFrame _af = new AminaFrame();
                    _af.pos = new Vector2(frames[i - 1].pos.x, frames[i - 1].pos.y);
                    _af.angle = frames[i - 1].angle;
                    frames.Add(_af);
                }
            }
        }
        else
        {
            Key _newKey = new Key(_fromIndex + _deltaIndex, _moveKey.type);
            Vector2 _pos = new Vector2(frames[_fromIndex].pos.x, frames[_fromIndex].pos.y);
            float _angle = frames[_fromIndex].angle;

            DeleteKeyFrame(_fromIndex);
            AddKeyFrame(_pos, _angle, _newKey);
        }

    }

    private void AddNewKeyAcceleration(Vector2 _pos, float _angle, Key _newKey)
    {

        if (keys.Count == 1)
        {
            frames = new List<AminaFrame>();

            for (int i = 0; i <= keys[0].index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames.Add(_af);
            }
            return;
        }

        int _index = _newKey.index;

        int k = GetIndexOf(_index);
        if (k == 0)
        {
            for (int i = 0; i <= _index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames[i] = _af;
            }

            int nextIndex = keys[k + 1].index;
            AminaFrame _end = frames[nextIndex];
            //float _deltaAng = (_end.angle - _angle) / (nextIndex - _index);

            int _deltaFrames = nextIndex - _index;

            float _startVX = 0;
            float _startVY = 0;
            float _startVA = 0;

            float aX = GetUniformAcceleration(_pos.x, _end.pos.x, _startVX, _deltaFrames);
            float aY = GetUniformAcceleration(_pos.y, _end.pos.y, _startVY, _deltaFrames);

            float _deltaAng = GetUniformAcceleration(_angle, _end.angle, _startVA, _deltaFrames);


            for (int i = 1; i < _deltaFrames; i++)
            {
                AminaFrame _af = new AminaFrame();
                float _realV = i - 0.5f;
                _af.pos = new Vector2(frames[_index + i - 1].pos.x + _startVX + _realV * aX, frames[_index + i - 1].pos.y + _startVY + _realV * aY);
                _af.angle = frames[_index + i - 1].angle + _startVA + _realV * _deltaAng;
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

                frames[_index + i] = _af;
            }

        }
        else
        if (k == keys.Count - 1)
        {
            int previousIndex = keys[k - 1].index;
            AminaFrame _end = new AminaFrame();
            _end.pos = new Vector2(_pos.x, _pos.y);
            _end.angle = _angle;
            //float _deltaAng = (_angle - frames[previousIndex].angle) / (_index - previousIndex);

            bool isAdd = false;
            if (_index > frames.Count - 1) isAdd = true;

            float _startX = frames[previousIndex].pos.x;
            float _startY = frames[previousIndex].pos.y;
            float _startA = frames[previousIndex].angle;

            int _deltaFrames = _index - previousIndex;

            float _startVX = 0;
            float _startVY = 0;
            float _startVA = 0;
            if (previousIndex > 0)
            {
                _startVX = frames[previousIndex].pos.x - frames[previousIndex - 1].pos.x;
                _startVY = frames[previousIndex].pos.y - frames[previousIndex - 1].pos.y;
                _startVA = frames[previousIndex].angle - frames[previousIndex - 1].angle;
            }


            float aX = GetUniformAcceleration(_startX, _pos.x, _startVX, _deltaFrames);
            float aY = GetUniformAcceleration(_startY, _pos.y, _startVY, _deltaFrames);

            float _deltaAng = GetUniformAcceleration(_startA, _angle, _startVA, _deltaFrames);


            for (int i = 1; i < _deltaFrames; i++)
            {
                AminaFrame _af = new AminaFrame();

                float _realV = i - 0.5f;
                _af.pos = new Vector2(frames[previousIndex + i - 1].pos.x + _startVX + _realV * aX, frames[previousIndex + i - 1].pos.y + _startVY + _realV * aY);
                //Debug.Log("pre x:" + frames[previousIndex + i - 1].pos.x);
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (_index - i + 1));
                _af.angle = frames[previousIndex + i - 1].angle + _startVA + _realV * _deltaAng;
                //_af.angle = frames[i - 1].angle + _deltaAng;

                if (isAdd)
                {
                    frames.Add(_af);
                }
                else
                {
                    frames[previousIndex + i] = _af;
                }
            }

            if (isAdd)
            {
                frames.Add(_end);
            }
            else
            {
                frames[_index] = _end;
            }
        }
        else
        {
            AminaFrame _key = new AminaFrame();
            _key.pos = new Vector2(_pos.x, _pos.y);
            _key.angle = _angle;

            frames[_index] = _key;
            InsertFrame(k);
            /*
            int previousIndex = keys[k - 1].index;
            AminaFrame _key = new AminaFrame();
            _key.pos = new Vector2(_pos.x, _pos.y);
            _key.angle = _angle;

            frames[_index] = _key;

            int _preDeltaFrames = _index - previousIndex;

            float _startX = frames[previousIndex].pos.x;
            float _startY = frames[previousIndex].pos.y;
            float _startA = frames[previousIndex].angle;
            float _startVX = 0;
            float _startVY = 0;
            float _startVA = 0;
            if (previousIndex > 0)
            {
                _startVX = frames[previousIndex].pos.x - frames[previousIndex - 1].pos.x;
                _startVY = frames[previousIndex].pos.y - frames[previousIndex - 1].pos.y;
                _startVA = frames[previousIndex].angle - frames[previousIndex - 1].angle;
            }

            float aX = GetUniformAcceleration(_startX, _pos.x, _startVX, _preDeltaFrames);
            float aY = GetUniformAcceleration(_startY, _pos.y, _startVY, _preDeltaFrames);

            float _deltaAng = GetUniformAcceleration(_startA, _angle, _startVA, _preDeltaFrames);


            for (int i = 1; i < _preDeltaFrames; i++)
            {
                AminaFrame _af = new AminaFrame();
                float _realV = i - 0.5f;
                _af.pos = new Vector2(frames[previousIndex + i - 1].pos.x + _startVX + _realV * aX, frames[previousIndex + i - 1].pos.y + _startVY + _realV * aY);
                _af.angle = frames[previousIndex + i - 1].angle + _startVA + _realV * _deltaAng;
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

                frames[previousIndex + i] = _af;
            }

            int nextIndex = keys[k + 1].index;
            AminaFrame _end = frames[nextIndex];


            int _nextDeltaFrames = nextIndex - _index;
            _startX = _pos.x;
            _startY = _pos.y;
            _startA = _angle;
            _startVX = _pos.x - frames[_index - 1].pos.x;
            _startVY = _pos.y - frames[_index - 1].pos.y;
            _startVA = _angle - frames[_index - 1].angle;
            aX = GetUniformAcceleration(_pos.x, _end.pos.x, _startVX, _nextDeltaFrames);
            aY = GetUniformAcceleration(_pos.y, _end.pos.y, _startVY, _nextDeltaFrames);
            _deltaAng = GetUniformAcceleration(_angle, _end.angle, _startVA, _nextDeltaFrames);

            for (int i = 1; i < _nextDeltaFrames; i++)
            {
                AminaFrame _af = new AminaFrame();
                float _realV = i - 0.5f;
                _af.pos = new Vector2(frames[_index + i - 1].pos.x + _startVX + _realV * aX, frames[_index + i - 1].pos.y + _startVY + _realV * aY);
                _af.angle = frames[_index + i - 1].angle + _startVA + _realV * _deltaAng;
                //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

                frames[_index + i] = _af;
            }*/

        }
    }


    void InsertFrame(int _InsertKeyIndex)
    {
        int _preIndex = keys[_InsertKeyIndex-1].index;

        Key _currentKey = keys[_InsertKeyIndex];
        switch (_currentKey.type)
        {
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_preIndex, _currentKey.index);
                break;
            case KeyType.LinearKey:
                LinearFillFrame(_preIndex, _currentKey.index);
                break;
            default:
                Debug.LogError("currentKey.type  not right");
                break;
        }

        Key _nextKey = keys[_InsertKeyIndex + 1];
        switch (_nextKey.type)
        {
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_currentKey.index, _nextKey.index);
                break;
            case KeyType.LinearKey:
                LinearFillFrame(_currentKey.index, _nextKey.index);
                break;
            default:
                Debug.LogError("nextKey.type  not right");
                break;
        }
    }

    void LinearFillFrame(int _start,int _end)
    {
        AminaFrame _endKey = frames[_end];
        AminaFrame _startKey = frames[_start];

        float _deltaAngle = (_endKey.angle - _startKey.angle) / (_end-_start);

        for (int i = _start + 1; i < _end; i++)
        {
            AminaFrame _af = new AminaFrame();
            _af.pos = Vector2.Lerp(frames[i - 1].pos, _endKey.pos, 1f / (_end - i + 1));
            _af.angle = frames[i - 1].angle + _deltaAngle;
            frames[i] = _af;
        }
    }

    void AccelerateFillFrame(int _start,int _end)
    {
        AminaFrame _startKey = frames[_start];
        AminaFrame _endKey = frames[_end];

        int _preDeltaFrames = _end - _start;

        float _startX = _startKey.pos.x;
        float _startY = _startKey.pos.y;
        float _startA = _startKey.angle;
        float _startVX = 0;
        float _startVY = 0;
        float _startVA = 0;
        if (_start > 0)
        {
            _startVX = _startKey.pos.x - frames[_start - 1].pos.x;
            _startVY = _startKey.pos.y - frames[_start - 1].pos.y;
            _startVA = _startKey.angle - frames[_start - 1].angle;
        }
        float aX = GetUniformAcceleration(_startX, _endKey.pos.x, _startVX, _preDeltaFrames);
        float aY = GetUniformAcceleration(_startY, _endKey.pos.y, _startVY, _preDeltaFrames);
        float _deltaAng = GetUniformAcceleration(_startA, _endKey.angle, _startVA, _preDeltaFrames);

        for (int i = 1; i < _preDeltaFrames; i++)
        {
            AminaFrame _af = new AminaFrame();
            float _realV = i - 0.5f;
            _af.pos = new Vector2(frames[_start + i - 1].pos.x + _startVX + _realV * aX, frames[_start + i - 1].pos.y + _startVY + _realV * aY);
            _af.angle = frames[_start + i - 1].angle + _startVA + _realV * _deltaAng;
            //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

            frames[_start + i] = _af;
        }


    }

    float GetUniformAcceleration(float _start,float _end,float _startSpeed, int _count)
    {
        float _acceleration = ((_end-_start) / _count - _startSpeed)*2 / _count;//加速度

        return _acceleration;
    }

    public int GetIndexOf(int k)
    {
        int i;
        for (i = 0; i < keys.Count; i++)
        {
            if (keys[i].index == k)
            {
                return i;
            }
        }
        Debug.Log("Wrong Index Num");
        return -1;
    }

    public bool IsTheIndexHaveKey(int _index)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].index == _index)
            {
                return true;
            }
        }
        return false;
    }

    public KeyType GetIndexType(int _index)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].index == _index)
            {
                return keys[i].type;
            }
        }
        Debug.LogError("not have the key");
        return KeyType.LinearKey;

    }

}

public enum KeyType
{
    LinearKey=0,
    AccelerateKey=1,
}

public class EditorAminaActionClip
{
    public int ProtypeID;

    public EditorAminaComponentClip[] componentClips;

}
