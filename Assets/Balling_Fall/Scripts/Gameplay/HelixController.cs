using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelixController : MonoBehaviour {

    [SerializeField] private HelixPieceController[] helixPieces;

    private bool movPieces = false;
    private bool randomValues = false;
    private bool oneDirection = false;
    private bool lerpMov = false;
    private int angleMov = 30;
    private float velMov = 1;
    private float velLerp = 0.2f;
    private float dstMeta = 1;
    private float percentMov = 20;

    public void HandleHelix(int disablePieces, int deadPieces, Color nomalPieceColor, Color deadPieceColor)
    {
        StartCoroutine(HandlingHelix(disablePieces, deadPieces, nomalPieceColor, deadPieceColor));
    }
    private IEnumerator HandlingHelix(int disablePieces, int deadPieces, Color nomalPieceColor, Color deadPieceColor)
    {
        List<HelixPieceController> listHelixPieceControl = new List<HelixPieceController>();
        foreach (HelixPieceController o in helixPieces)
        {
            listHelixPieceControl.Add(o);
        }

        //Handle disable pieces
        while (disablePieces > 0)
        {
            int index = Random.Range(0, listHelixPieceControl.Count);
            listHelixPieceControl[index].Disable();
            listHelixPieceControl.Remove(listHelixPieceControl[index]);
            disablePieces--;
            yield return null;
        }

        //Handle dead pieces
        while (deadPieces > 0)
        {
            int index = Random.Range(0, listHelixPieceControl.Count);

            if (movPieces)
            {
                bool isMovable = Random.Range(1, 100) <= percentMov ? true : false;
                if (randomValues)
                {
                    oneDirection = Random.Range(0, 2) == 0 ? false : true;
                    lerpMov = Random.Range(0, 2) == 0 ? false : true;
                }
                if(isMovable) listHelixPieceControl[index].SetDeadPieceWithMov(oneDirection, lerpMov, angleMov, velMov, velLerp, dstMeta);
                else listHelixPieceControl[index].SetDeadPiece();
            }
            else
            {
                listHelixPieceControl[index].SetDeadPiece();
            }
            
            listHelixPieceControl.Remove(listHelixPieceControl[index]);
            deadPieces--;
            yield return null;
        }

        //Handle normal pieces
        foreach (HelixPieceController o in listHelixPieceControl)
        {
            o.SetNormalPiece();
        }
    }

    public void ShatterAllPieces()
    {
        foreach(HelixPieceController o in helixPieces)
        {
            o.Shatter();
        }
        GameManager.Instance.CreateFadingHelix(transform.position);
    }

    public void SetMovPiece(bool _randomValues, bool _oneDirection, bool _lerpMov, int _angleMov, float _vel, float _velLerp, float _dstMeta, int _percentMov)
    {
        movPieces = true;
        randomValues = _randomValues;
        oneDirection = _oneDirection;
        lerpMov = _lerpMov;
        angleMov = _angleMov;
        velMov = _vel;
        velLerp = _velLerp;
        dstMeta = _dstMeta;
        percentMov = _percentMov;
    }
}
