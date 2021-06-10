using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSpriteFrame_Container
{
    public bool isKeyFrame { get { return isKeySprite || isKeyAlpha; } }
    public bool isKeySprite;
    public bool isKeyAlpha;
    public bool isKeyFace;

    public Sprite sprite;
    public Vector3 facePos;
    private int alpha;
    public int Alpha
    {
        get { return alpha; }
        set
        {
            if (value > 100) alpha = 100;
            else if (value < 0) alpha = 0;
            else alpha = value;
        }
    }


    public EditorSpriteFrame_Container()
    {
        isKeySprite = false;
        isKeyAlpha = false;
        isKeyFace = false;
    }

    public void SetNewKeyFrame(EditorSpriteFrame _spriteFrame)
    {
        isKeySprite = true;
        isKeyAlpha = true;
        sprite = _spriteFrame.sprite;
        Alpha = _spriteFrame.alpha;
        facePos = _spriteFrame.facePos;
    }

    public void SetNewTransitionFrame(EditorSpriteFrame _prevFrame, EditorSpriteFrame _nextFrame,int _index)
    {
        isKeySprite = false;
        isKeyAlpha = false;
        sprite = _prevFrame.sprite;
        Alpha = (int)(_prevFrame.alpha + (_nextFrame.alpha - _prevFrame.alpha) * ((float)(_index - _prevFrame.index) / (_nextFrame.index - _prevFrame.index)));
    }

    public void SetTransitonAlpha(int _prevAlpha, int _nextAlpha,int _prevIndex,int _nextIndex,int _index)
    {

        Alpha = (int)(_prevAlpha + (_nextAlpha - _prevAlpha) * ((float)(_index - _prevIndex) / (_nextIndex - _prevIndex)));
    }

    public void SetFacePos(Vector2 _pos)
    {
        facePos = _pos;
    }

    public static Vector3 GetFacePos(Sprite _sp,Color _left,Color _right)
    {
        Texture2D t2d = _sp.texture;
        List<Vector2Int> v2 = new List<Vector2Int>();
        List<Vector2Int> v2_2 = new List<Vector2Int>();
        Color targetCol = _left;
        Color targetCol2 = _right;

        int x, y;
        int xMax = Mathf.RoundToInt(_sp.rect.xMax);
        int yMax = Mathf.RoundToInt(_sp.rect.yMax);
        for (x = Mathf.RoundToInt(_sp.rect.x); x <= xMax; x++)
        {
            for (y = Mathf.RoundToInt(_sp.rect.y); y <= yMax; y++)
            {
                Color col = t2d.GetPixel(x, y);
                //Color _dC = col - targetCol;
                float _d = Mathf.Abs(col.r - targetCol.r) + Mathf.Abs(col.g - targetCol.g) + Mathf.Abs(col.b - targetCol.b);
                float _d2 = Mathf.Abs(col.r - targetCol2.r) + Mathf.Abs(col.g - targetCol2.g) + Mathf.Abs(col.b - targetCol2.b);
                //Debug.Log(_sp.name + "d:" + _d);

                if (_d < 0.15 && col.a > 0.9f)
                {
                    v2.Add(new Vector2Int(x, y));
                    Debug.Log(_sp.name+"v2:" + new Vector2Int(x, y));
                }

                if (_d2 < 0.15 && col.a > 0.9f)
                {
                    v2_2.Add(new Vector2Int(x, y));
                    Debug.Log(_sp.name + "v2_2:" + new Vector2Int(x, y));
                }
            }
        }

        if (v2.Count > 0 && v2_2.Count > 0)
        {
            float _sx1=0;
            float _sy1 = 0;
            for(int i = 0; i < v2.Count; i++)
            {
                _sx1 += v2[i].x;
                _sy1 += v2[i].y;
            }
            _sx1 = _sx1 / v2.Count;
            _sy1 = _sy1 / v2.Count;

            float _sx2 = 0;
            float _sy2 = 0;
            for (int i = 0; i < v2_2.Count; i++)
            {
                _sx2 += v2_2[i].x;
                _sy2 += v2_2[i].y;
            }
            _sx2 = _sx2 / v2_2.Count;
            _sy2 = _sy2 / v2_2.Count;


            Vector2 _vt = new Vector2(_sx1 - _sp.rect.x, _sy1 - _sp.rect.y);

            Vector2 _vt2 = new Vector2(_sx2 - _sp.rect.x, _sy2 - _sp.rect.y);

            float _x = (-_sp.pivot.x + (_vt2.x + _vt.x) / 2) / _sp.pixelsPerUnit;
            float _y = (-_sp.pivot.y + (_vt.y + _vt2.y) / 2) / _sp.pixelsPerUnit;

            Vector2 _va = _vt2 - _vt;

            float _ag = Vector2.SignedAngle( Vector2.right, _va);
            if (_ag > 0)
            {
                while (_ag > 360)
                {
                    _ag -= 360;
                }
            }
            else
            {
                while (_ag < -0)
                {
                    _ag += 360;
                }
            }
            return new Vector3(_x, _y, _ag);
        }
        else
        {
            string _errCol = "";

            if (v2.Count == 0)
            {
                _errCol += " " + _left.ToString();
            }

            if (v2_2.Count == 0)
            {
                _errCol += " " + _right.ToString();
            }

            Debug.LogError(_sp.name+"can not find the target color pixel. color="+ _errCol);

            return Vector3.zero;
        }

    }


}
