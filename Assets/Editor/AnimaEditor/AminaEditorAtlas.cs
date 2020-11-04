using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AminaEditorAtlas
{
    public EditorAminaAtlas atlas;
    public SpriteRenderer spriteRenderer;

    List<Sprite> allSprite;
    List<float> allAlpha;
    List<int> examinData;

    public AminaEditorAtlas(SpriteRenderer _r)
    {
        spriteRenderer = _r;
    }

    public SpriteFrame GetSpriteFrame(int _index)
    {
        if (atlas != null)
        {
            for (int i = 0; i < atlas.sprites.Length; i++)
            {
                if (_index == atlas.sprites[i].index)
                {
                    return atlas.sprites[i];
                }
            }
        }
        
        return null;
    }

    public void GetSprite(int frameIndex)
    {
        if (allSprite != null && frameIndex< allSprite.Count)
        {
            spriteRenderer.sprite= allSprite[frameIndex];
            spriteRenderer.color = new Color(1,1,1,allAlpha[frameIndex]);
        }
    }

    public void LoadEditorAminaAtlas(EditorAminaAtlas _atlas)
    {
        atlas = _atlas;

        List<SpriteFrame> _sprs=new List<SpriteFrame>();

        for(int i = 0; i < _atlas.sprites.Length; i++)
        {
            SpriteFrame t = _atlas.sprites[i];

            _sprs.Add(t);

            SpriteFrame temp = t;
            for (int j = _sprs.Count - 2; j >= 0; j--)
            {
                if (temp.index < _sprs[j].index)
                {
                    _sprs[j + 1] = _sprs[j];
                    _sprs[j] = temp;
                }
                else { break; }
            }
        }
        atlas.sprites = _sprs.ToArray();

        int max = _sprs.Count - 1;

        examinData = new List<int>();
        for(int i=0;i<= max; i++)
        {
            examinData.Add(_sprs[i].index);
        }

        allSprite = new List<Sprite>();
        allAlpha = new List<float>();
        int _spriteMax = _sprs[max].index;
        int _index = 0;
        if (_sprs.Count > 1)
        {
            float _preKeyAlpha = _sprs[0].alpha;
            float _deltaAlpha = (_sprs[1].alpha - _preKeyAlpha) / (_sprs[1].index - _sprs[0].index);
            float _alpha = _preKeyAlpha;
            for (int i = 0; i < _spriteMax; i++)
            {
                if (i == _sprs[_index + 1].index)
                {
                    _index++;
                    _alpha = _sprs[_index].alpha;
                    _deltaAlpha = (_sprs[_index + 1].alpha - _sprs[_index].alpha) / (_sprs[_index + 1].index - _sprs[_index].index);
                }
                else
                {
                    _alpha += _deltaAlpha; 
                }
                allSprite.Add(_sprs[_index].sprite);
                allAlpha.Add(_alpha);
            }
            allSprite.Add(_sprs[_index + 1].sprite);
            allAlpha.Add(_sprs[_index + 1].alpha);
        }
        else if(_sprs.Count > 0)
        {
            allSprite.Add(_sprs[0].sprite);
            allAlpha.Add(_sprs[0].alpha);
        }
        
    }

    public bool Examin()
    {
        if (examinData == null) return false;
        int exCount = examinData.Count;

        if (exCount == atlas.sprites.Length)
        {
            for(int i = 0; i < exCount; i++)
            {
                if(examinData[i]!= atlas.sprites[i].index)
                {
                    UpdateData();
                    return true;
                }
            }
        }
        else
        {
            UpdateData();
            return true;
        }

        return false;
    }


    private void UpdateData()
    {
        Debug.Log("atlas UpdateData");

        List<SpriteFrame> _sprs = new List<SpriteFrame>();

        for (int i = 0; i < atlas.sprites.Length; i++)
        {
            SpriteFrame t = atlas.sprites[i];

            _sprs.Add(t);

            SpriteFrame temp = t;
            for (int j = _sprs.Count - 2; j >= 0; j--)
            {
                if (temp.index < _sprs[j].index)
                {
                    _sprs[j + 1] = _sprs[j];
                    _sprs[j] = temp;
                }
                else { break; }
            }
        }
        atlas.sprites = _sprs.ToArray();

        examinData = new List<int>();
        allSprite = new List<Sprite>();
        for (int i = 0; i < _sprs.Count; i++)
        {
            examinData.Add(_sprs[i].index);
        }
        
        allSprite = new List<Sprite>();
        allAlpha = new List<float>();
        int _spriteMax = _sprs[_sprs.Count - 1].index;
        int _index = 0;
        if (_sprs.Count > 1)
        {
            float _preKeyAlpha = _sprs[0].alpha;
            float _deltaAlpha = (_sprs[1].alpha - _preKeyAlpha) / (_sprs[1].index - _sprs[0].index);
            float _alpha = _preKeyAlpha;
            for (int i = 0; i < _spriteMax; i++)
            {
                if (i == _sprs[_index + 1].index)
                {
                    _index++;
                    _alpha = _sprs[_index].alpha;
                    _deltaAlpha = (_sprs[_index+1].alpha - _sprs[_index].alpha) / (_sprs[_index+1].index - _sprs[_index].index);
                }
                else
                {
                    _alpha += _deltaAlpha;
                }
                allSprite.Add(_sprs[_index].sprite);
                allAlpha.Add(_alpha);
            }
            allSprite.Add(_sprs[_index + 1].sprite);
            allAlpha.Add(_sprs[_index + 1].alpha);
        }
        else if (_sprs.Count > 0)
        {
            allSprite.Add(_sprs[0].sprite);
            allAlpha.Add(_sprs[0].alpha);
        }


    }
}

public class SpriteFrameContainer : Editor
{
    public SpriteFrame sprite;

}
