using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelixPieceController : MonoBehaviour {

    private Rigidbody rigid = null;
    private MeshCollider meshCollider = null;
    private MeshRenderer meshRender = null;
    private Renderer render = null;
    
    private void CacheComponents()
    {
        //Cache components
        if (rigid == null)
            rigid = GetComponent<Rigidbody>();
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();
        if (meshRender == null)
            meshRender = GetComponent<MeshRenderer>();
        if (render == null)
            render = GetComponent<Renderer>();
    }

    /// <summary>
    /// Disable this piece
    /// </summary>
    public void Disable()
    {
        CacheComponents();
        meshRender.enabled = false;
        meshCollider.enabled = false;
    }


    /// <summary>
    /// Set this helix piece as dead piece
    /// </summary>
    public void SetDeadPiece()
    {
        CacheComponents();
        gameObject.tag = "Finish";
        meshRender.material = GameManager.Instance.DeadPieceMaterial;
    }
    public void SetDeadPieceWithMov(bool oneDirection, bool lerpMov, int angleMov, float vel, float velLerp, float dstMeta)
    {
        CacheComponents();
        gameObject.tag = "Finish";
        meshRender.material = GameManager.Instance.DeadPieceMaterial;

        transform.localScale += Vector3.one * 0.01f;

        StartCoroutine(MovPiece(oneDirection, lerpMov, angleMov, vel, velLerp, dstMeta));
    }

    IEnumerator MovPiece(bool oneDirection, bool lerpMov, int angleMov, float vel, float velLerp, float dstMeta)
    {
        float initialAngle = transform.localEulerAngles.y;
        float totalRotacion = angleMov;
        float tLerp = 0;
        float rotTarget = 0;
        transform.localEulerAngles = new Vector3(transform.eulerAngles.x, initialAngle + totalRotacion, transform.eulerAngles.z);

        while (true)
        {
            if (oneDirection)
            {
                if (lerpMov)
                {
                    totalRotacion = 0;
                    while (true)
                    {
                        tLerp = 0;
                        rotTarget = totalRotacion + angleMov;
                        while (totalRotacion < rotTarget - dstMeta)
                        {
                            tLerp += velLerp * Time.deltaTime;
                            totalRotacion = Mathf.Lerp(totalRotacion, rotTarget, tLerp);
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                            yield return null;
                        }
                        yield return null;
                    }
                }
                else
                {
                    while (true)
                    {
                        totalRotacion += vel * Time.deltaTime;
                        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                        yield return null;
                    }
                }
            }
            else
            {
                if (lerpMov)
                {
                    while (true)
                    {
                        tLerp = 0;
                        while (totalRotacion < angleMov - dstMeta)
                        {
                            tLerp += velLerp * Time.deltaTime;
                            totalRotacion = Mathf.Lerp(totalRotacion, angleMov, tLerp);
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                            yield return null;
                        }
                        tLerp = 0;
                        while (totalRotacion > -angleMov + dstMeta)
                        {
                            tLerp += velLerp * Time.deltaTime;
                            totalRotacion = Mathf.Lerp(totalRotacion, -angleMov, tLerp);
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                            yield return null;
                        }
                        yield return null;
                    }
                }
                else
                {
                    while (true)
                    {
                        while (totalRotacion < angleMov - 0.02f)
                        {
                            totalRotacion += vel * Time.deltaTime;
                            if (totalRotacion >= angleMov) totalRotacion = angleMov - 0.01f;
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                            yield return null;
                        }
                        while (totalRotacion >  -angleMov + 0.02f)
                        {
                            totalRotacion -= vel * Time.deltaTime;
                            if (totalRotacion < -angleMov) totalRotacion = -angleMov + 0.01f;
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, initialAngle + totalRotacion, transform.localEulerAngles.z);
                            yield return null;
                        }
                        yield return null;
                    }
                }
            }
            yield return null;
        }
        
    }

    /// <summary>
    /// Set this helix piece as normal piece
    /// </summary>
    public void SetNormalPiece()
    {
        CacheComponents();
        meshRender.material = GameManager.Instance.NormalPieceMaterial;
    }


    public void Shatter()
    {
        if (meshRender.enabled)
        {
            StartCoroutine(Shattering());
        }    
    }
    private IEnumerator Shattering()
    {
        meshRender.material = GameManager.Instance.BrokenPieceMaterial;
        meshCollider.enabled = false;
        Vector3 forcePoint = transform.parent.position;
        transform.parent = null;
        Vector3 point_1 = transform.position;
        Vector3 point_2 = meshRender.bounds.center + Vector3.up * (meshRender.bounds.size.y / 2f);
        Vector3 dir = (point_2 - point_1).normalized;
        rigid.isKinematic = false;
        rigid.AddForceAtPosition(dir * 10f, forcePoint, ForceMode.Impulse);
        rigid.AddTorque(Vector3.left * 100f);
        rigid.velocity = Vector3.down * 10f;
        yield return new WaitForSeconds(5f);
        rigid.isKinematic = true;
        Disable();
    }

}
