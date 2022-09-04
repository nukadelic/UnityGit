using UnityEngine;

public static class Rotation
{
    #region Swing Twist 

    public static void SwingTwist( Quaternion q, Vector3 twistAxis, out Quaternion swing, out Quaternion twist )
    {
        // https://allenchou.net/2018/05/game-math-swing-twist-interpolation-sterp/

        Vector3 r = new Vector3( q.x , q.y , q.z );

        // singularity: rotation by 180 degree
        if (r.sqrMagnitude < Mathf.Epsilon)
        {
            Vector3 rotatedTwistAxis = q * twistAxis;
            Vector3 swingAxis = 
            Vector3.Cross(twistAxis, rotatedTwistAxis);

            if (swingAxis.sqrMagnitude > Mathf.Epsilon)
            {
                float swingAngle = 
                Vector3.Angle(twistAxis, rotatedTwistAxis);
                swing = Quaternion.AngleAxis(swingAngle, swingAxis);
            }
            else
            {
                // more singularity: 
                // rotation axis parallel to twist axis
                swing = Quaternion.identity; // no swing
            }

            // always twist 180 degree on singularity
            twist = Quaternion.AngleAxis(180.0f, twistAxis);
            return;
        }

        // this piece of code looks similar to `getSwingTwist()` from
        // https://github.com/libgdx/libgdx/blob/master/gdx/src/com/badlogic/gdx/math/Quaternion.java
        // https://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/

        // meat of swing-twist decomposition
        Vector3 p = Vector3.Project( r , twistAxis );
        twist = new Quaternion( p.x, p.y, p.z, q.w );
        twist = twist.normalized;
        swing = q * Quaternion.Inverse( twist );
    }

    #endregion

    #region Twist 

    public static Quaternion OrthoX = Quaternion.Euler(90 * Mathf.Deg2Rad, 0, 0);
    public static Quaternion OrthoY = Quaternion.Euler(0, 90 * Mathf.Deg2Rad, 0);

    public static float TwistSigned( Quaternion q, Vector3 axis )
    {
        // modified version , using hax ( i don't remember how i solved it ) 

        Orthonormals(axis, out Vector3 n1, out Vector3 n2);

        Vector3 t1 = q * n1;
        Vector3 flat1 = t1 - (Vector3.Dot(t1, axis) * axis);
        float angle1 = Vector3.SignedAngle(n1, flat1.normalized, axis);

        Vector3 t2 = q * n2;
        Vector3 flat2 = t2 - (Vector3.Dot(t2, axis) * axis);
        float angle2 = Vector3.SignedAngle(n2, flat2.normalized, axis);

        var s1 = Mathf.Sign(angle1);
        var s2 = Mathf.Sign(angle2);
        var m = Mathf.Min(Mathf.Abs(angle1), Mathf.Abs(angle2));
        return s1 == s2 ? s1 * m : s1 * s2 * m;
    }

    public static float Twist( Quaternion q, Vector3 axis )
    {
        // https://stackoverflow.com/a/4341489

        axis.Normalize();
        // Get the plane the axis is a normal of
        Orthonormals( axis, out Vector3 n1, out Vector3 n2 ); 
        Vector3 t1 = q * n1;
        // Project transformed vector onto plane
        Vector3 flat1 = t1 - (Vector3.Dot(t1, axis) * axis);
        // Get angle between original vector and projected transform to get angle around normal
        var angle1 = (float)Mathf.Acos(Vector3.Dot(n1, flat1.normalized));

        // Use second value for error correction
        Vector3 t2 = q * n2;
        Vector3 flat2 = t2 - (Vector3.Dot(t2, axis) * axis);
        var angle2 = Mathf.Acos(Vector3.Dot(n2, flat2.normalized));

        return Mathf.Min( angle1 , angle2 ) * Mathf.Rad2Deg;
    }
    public static void Orthonormals( Vector3 normal, out Vector3 n1 , out Vector3 n2 )
    {
        Vector3 w = OrthoX * normal;
        float dot = Vector3.Dot(normal, w);
        if (Mathf.Abs(dot) > 0.6f) w = OrthoY * normal;
        n1 = Vector3.Cross(normal, w.normalized).normalized;
        n2 = Vector3.Cross(normal, n1).normalized;
    }


    #endregion

    #region Pitch Roll Yaw

    public static Quaternion PitchRollYaw( float pitch, float roll, float yaw, Transform space )
    {
        return Quaternion.AngleAxis( pitch, space.right ) *
            Quaternion.AngleAxis( yaw, space.up ) *
            Quaternion.AngleAxis( roll, space.forward );
    }

    // reference : https://answers.unity.com/questions/416169/finding-pitchrollyaw-from-quaternions.html

    public static Quaternion PitchRollYaw( float pitch, float roll, float yaw )
    {
        return Quaternion.AngleAxis( pitch, Vector3.right ) *
            Quaternion.AngleAxis( yaw, Vector3.up ) *
            Quaternion.AngleAxis( roll, Vector3.forward );
    }

    public static float Pitch( Quaternion q ) => Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
    public static float Roll( Quaternion q ) => Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
    public static float Yaw( Quaternion q ) => Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);

    #endregion
}
