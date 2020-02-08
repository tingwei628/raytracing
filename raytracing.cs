using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Raytracing {
 public class Vec3 {
  public Vec3() {}
  public Vec3(double x, double y, double z) {
   V = new double[3];
   V[0] = x;
   V[1] = y;
   V[2] = z;
   Length2 = x * x + y * y + z * z;
   Length = Math.Sqrt(Length2);
  }
  public double[] V {
   get;
   set;
  }
  public double Length2 {
   get;
   set;
  }
  public double Length {
   get;
   set;
  }
  public static Vec3 operator + (Vec3 self, Vec3 a) => new Vec3(self.V[0] + a.V[0], self.V[1] + a.V[1], self.V[2] + a.V[2]);
  public static Vec3 operator - (Vec3 self, Vec3 a) => new Vec3(self.V[0] - a.V[0], self.V[1] - a.V[1], self.V[2] - a.V[2]);
  

  public static Vec3 operator * (Vec3 self, double a) => new Vec3(self.V[0] * a, self.V[1] * a, self.V[2] * a);
  public static Vec3 operator / (Vec3 self, double a) => new Vec3(self.V[0] * 1 / a, self.V[1] * 1 / a, self.V[2] * 1 / a);


 }
 public class Sphere {
  public Sphere() {}
  public Sphere(double x, double y, double z, double r, double[] color, double spec) {
   // center of X Y Z
   X = x;
   Y = y;
   Z = z;
   R = r;
   ColorR = color[0];
   ColorG = color[1];
   ColorB = color[2];
   Spec = spec;
  }
  public double X {
   get;
   set;
  }
  public double Y {
   get;
   set;
  }
  public double Z {
   get;
   set;
  }
  public double R {
   get;
   set;
  }
  //public int R_2 => X*X + Y*Y+Z*Z;

  public string Color {
   get {
     return  $"{ColorR} {ColorG} {ColorB}";
   }
   
  }
  public double ColorR {
   get;
   set;
  }
  public double ColorG {
   get;
   set;
  }
  public double ColorB {
   get;
   set;
  }
  public double T1 {
   get;
   set;
  } = Double.MaxValue;
  public double T2 {
   get;
   set;
  } = Double.MaxValue;
  public double Spec {
   get;
   set;
  }
 }
 public class ViewPort {
  public double VW {
   get;
   set;
  }
  public double VH {
   get;
   set;
  }
  public ViewPort(double vw, double vh) {
   VW = vw;
   VH = vh;
  }
 }

 public class Light {
  public string Type {
   get;
   set;
  }
  public double Intensity {
   get;
   set;
  }
  public Vec3 Vec {
   get;
   set;
  }
  public Light(string type, double intensity, Vec3 v = null) {
   Type = type;
   Intensity = intensity;
   if (v != null) Vec = v;
  }
 }
 public class Canvas {
  public double CW {
   get;
   set;
  }
  public double CH {
   get;
   set;
  }
  public ViewPort VP {
   get;
  }
  public Canvas(double cw, double ch, ViewPort vp) {
   CW = cw;
   CH = ch;
   VP = vp;
  }
  public Vec3 CanvasToViewPort(double cx, double cy, double cz = 1.0) => new Vec3(VP.VW * cx * 1 / CW, VP.VH * cy * 1 / CH, cz);

 }
 public static class Util {
  public static Sphere Copy(this Sphere sphere) { return new Sphere(sphere.X, sphere.Y, sphere.Z, sphere.R, new double[] { sphere.ColorR, sphere.ColorG, sphere.ColorB }, sphere.Spec); }
  public static double dot(Vec3 self, Vec3 a) => (self.V[0] * a.V[0] + self.V[1] * a.V[1] + self.V[2] * a.V[2]);
  public static Vec3 unit(Vec3 self) => self / self.Length;

  public static double[] Interfere(Vec3 origin, Vec3 D, Sphere sphere) {
   double[] t1_t2 = new double[2];
   //  C = sphere.center
    //Console.WriteLine("sphere closed_sphere.Color {0}", sphere.Color);
   Vec3 c = new Vec3(sphere.X, sphere.Y, sphere.Z);
   // r = sphere.radius
   double r = sphere.R;
   // OC = O - C
   Vec3 OC =  origin - c;
   double k1 = dot(D, D);
   double k2 = 2 * dot(OC, D);
   double k3 = dot(OC, OC) - r * r;
   double discriminant = k2 * k2 - 4 * k1 * k3;
   if (discriminant < 0) {
    t1_t2[0] = Double.MaxValue;
    t1_t2[1] = Double.MaxValue;
    return t1_t2;
   }
   t1_t2[0] = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
   t1_t2[1] = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
   return t1_t2;
  }
  // return Sphere color
  public static string TraceRay(Vec3 origin, Vec3 light, double t_min, double t_max, List < Sphere > spheres, List < Light > lights) {
   double closed_t = Double.MaxValue;
   Sphere closed_sphere = null;
   double[] t1_t2 = new double[2];
   foreach(var sphere in spheres) {
    t1_t2 = Interfere(origin, light, sphere);
    // t1
    //Console.WriteLine("t1_t2 {0} {1}", t1_t2[0], t1_t2[1]);
    //Console.WriteLine("before sphere.Color {0}", sphere.Color);
    if (t_min <= t1_t2[0] && t1_t2[0] < t_max && t1_t2[0] < closed_t) {
     closed_sphere = sphere.Copy();
     closed_t = t1_t2[0];
     closed_sphere.T1 = closed_t;
    }
    // t2
    if (t_min <= t1_t2[1] && t1_t2[1] < t_max && t1_t2[1] < closed_t) {
     closed_sphere = sphere.Copy();
     closed_t = t1_t2[1];
     closed_sphere.T2 = closed_t;
    }
   }
   if (closed_sphere == null) return "255 255 255";

   double closest_t = Math.Min(closed_sphere.T1, closed_sphere.T2);
   Vec3 P = origin + light * closest_t;
   Vec3 N1 = P - new Vec3(closed_sphere.X, closed_sphere.Y, closed_sphere.Z);
   Vec3 N = Util.unit(N1);
   //Console.WriteLine("before closed_sphere.Color {0}", closed_sphere.Color);
   double intensity = Util.ComputeLight(P, N, lights, light * -1, closed_sphere.Spec);
   
   closed_sphere.ColorR = Math.Ceiling(closed_sphere.ColorR * intensity);
   closed_sphere.ColorG = Math.Ceiling(closed_sphere.ColorG * intensity);
   closed_sphere.ColorB = Math.Ceiling(closed_sphere.ColorB * intensity);
    //Console.WriteLine("=> {0} {1} {2}",  light.V[0],  light.V[1], light.V[2]);
   //Console.WriteLine("closed_sphere.Color {0}", closed_sphere.Color);
   return closed_sphere.Color;
  }

  public static double ComputeLight(Vec3 P, Vec3 N, List < Light > lights, Vec3 V, double spec) {
   double intensity = 0.0;
   foreach(var light in lights) {
    if (light.Type == "ambient") intensity += light.Intensity;
    else {
     Vec3 L = new Vec3(0, 0, 0);
     if (light.Type == "point") L = light.Vec - P;
     else L = light.Vec; // type == directional

     double n_dot_l = Util.dot(N, L);
     if (n_dot_l > 0) {
      intensity += light.Intensity * n_dot_l / (N.Length * L.Length);
     }

     if (spec != -1) {
      Vec3 R = N * n_dot_l * 2.0 - L;
      double r_dot_v = Util.dot(R, V);
      if (r_dot_v > 0) intensity += light.Intensity * Math.Pow(r_dot_v / (R.Length * V.Length), spec);
     }
    }
   }
   return intensity;
  }
 }
 public static class MainClass {
  public static void Main(string[] args) {
   ViewPort vp = new ViewPort(1, 1);
   Canvas cv = new Canvas(250, 250, vp);
   double cw = cv.CW;
   double ch = cv.CH;

   StringBuilder ppm = new StringBuilder();
   ppm.AppendLine($"P3\n{cw} {ch}\n255");
   StringBuilder sb = new StringBuilder();

   Vec3 origin = new Vec3(0, 0, 0);
   // red


   Sphere sh1 = new Sphere(0, -1, 3, 1, new double[] {
    255,
    0,
    0
   }, 500);

   // blue
   Sphere sh2 = new Sphere(2, 0, 4, 1, new double[] {
    0,
    0,
    255
   }, 500);
   // green
   Sphere sh3 = new Sphere(-2, 0, 4, 1, new double[] {
    0,
    255,
    0
   }, 10);
   List < Sphere > spheres = new List < Sphere > {
    sh1,
    sh2,
    sh3
   };

   Light light1 = new Light("ambient", 0.2);
   Light light2 = new Light("point", 0.6, new Vec3(2, 1, 0));
   Light light3 = new Light("directional", 0.2, new Vec3(1, 4, 4));
   List < Light > lights = new List < Light > {
    light1,
    light2,
    light3
   };
   double start_y = -ch / 2.0;
   double end_y = ch / 2.0;
   double start_x = -cw / 2.0;
   double end_x = cw / 2.0;
   for (double y = start_y; y <= end_y; y = y + 1) {
    for (double x = start_x; x <= end_x; x = x + 1) {
     
     Vec3 D = cv.CanvasToViewPort(x, y);
     //Console.WriteLine("{0} {1} {2}", D.V[0], D.V[1], D.V[2]);
     // t_min = 1
     // t_max = Double.MaxValue
     var color = Util.TraceRay(origin, D, 1, Double.MaxValue, spheres, lights);
     sb.AppendLine(color);
    }
   }
   ppm.Append(sb.ToString());
   File.WriteAllText("./raytracing.ppm", ppm.ToString());
  }
 }
}
