using UnityEngine;
//struct
public class Coordinate
{
    //x (m),y (m),Density (kg/m3),Pressure (Pa),Temperature (K),Vel_x (m/s),Vel_y (m/s)
    //public float x;
    //public float y;
    //public float vel_x;
    //public float vel_y;
    //public float density;
    //public float pressure;
    public Vector3 pos;
    public float parameter;

    public Coordinate(/*float pX, float pY, float pDensity,*/ Vector3 pos, float parameter /*, float pVel_x, float pVel_y*/)
    {
        //this.x = pX;
        //this.y = pY;
        //this.vel_x = pVel_x;
        //this.vel_y = pVel_y;
        //this.density = pDensity;
        this.pos = pos;
        this.parameter = parameter;
    }
}
