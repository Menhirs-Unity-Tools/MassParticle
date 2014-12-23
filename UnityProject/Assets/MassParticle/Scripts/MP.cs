﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


[StructLayout(LayoutKind.Explicit)]
public struct MPParticle
{
    [FieldOffset(0)]  public Vector3 position;
    [FieldOffset(12)] public uint id;
    [FieldOffset(16)] public Vector3 velocity;
    [FieldOffset(28)] public float speed;
    [FieldOffset(32)] public float density;
    [FieldOffset(36)] public float lifetime;
    [FieldOffset(40)] public UInt16 hit;
    [FieldOffset(42)] public UInt16 hit_prev;
    [FieldOffset(44)] public int userdata;
};

[StructLayout(LayoutKind.Explicit)]
public struct MPForceData
{
    [FieldOffset(0)]  public Vector3 position;
    [FieldOffset(16)] public Vector3 velocity;
    [FieldOffset(28)] public float speed;
    [FieldOffset(32)] public int num_hits;
    [FieldOffset(36)] public int pad1;
    [FieldOffset(40)] public int pad2;
    [FieldOffset(44)] public int pad3;
}


public struct MPKernelParams
{
    public Vector3 world_center;
    public Vector3 world_size;
    public int world_div_x;
    public int world_div_y;
    public int world_div_z;
    public Vector3 scaler;

    public int solver_type;
    public int enable_interaction;
    public int enable_colliders;
    public int enable_forces;
    public int id_as_float;

    public float timestep;
    public float decelerate;
    public float advection;
    public float pressure_stiffness;

    public int max_particles;
    public float particle_size;

    public float SPHRestDensity;
    public float SPHParticleMass;
    public float SPHViscosity;

    public float RcpParticleSize2;
    public float SPHDensityCoef;
    public float SPHGradPressureCoef;
    public float SPHLapViscosityCoef;
};

public enum MPSolverType
{
    Impulse = 0,
    SPH = 1,
    SPHEstimate = 2,
}
public enum MPUpdateMode
{
    Immediate = 0,
    Deferred = 1,
}

public delegate void MPHitHandler(ref MPParticle particle);
public delegate void MPForceHandler(ref MPForceData force);

public struct MPColliderProperties
{
    public int owner_id;
    public float stiffness;
    public MPHitHandler hit_handler;
    public MPForceHandler force_handler;

    public void SetDefaultValues()
    {
        owner_id = 0;
        stiffness = 1500.0f;
        hit_handler = null;
        force_handler = null;
    }
}


public enum MPForceShape
{
    All,
    Sphere,
    Capsule,
    Box
}

public enum MPForceDirection
{
    Directional,
    Radial,
    RadialCapsule,
    VectorField,
}

public struct MPForceProperties
{
    public MPForceShape shape_type;
    public MPForceDirection dir_type;
    public float strength_near;
    public float strength_far;
    public float range_inner;
    public float range_outer;
    public float attenuation_exp;

    public Vector3 directional_pos;
    public Vector3 directional_dir;
    public Vector3 radial_center;

    public void SetDefaultValues()
    {
        attenuation_exp = 0.25f;
    }
}

public unsafe struct MPMeshData
{
    public int* indices;
    public Vector3* vertices;
    public Vector3* normals;
    public Vector2* uv;
};


public struct MPSpawnParams
{
    public Vector3 velocity;
    public float velocity_random_diffuse;
    public float lifetime;
    public float lifetime_random_diffuse;
    public int userdata;
    public MPHitHandler handler;
}

public class MPAPI {
    
    [DllImport ("MassParticleHelper")] unsafe public static extern void mphInitialize();

    [DllImport ("MassParticle")] public static extern void mpGeneratePointMesh(int context, int i, ref MPMeshData md);
    [DllImport ("MassParticle")] public static extern void mpGenerateCubeMesh(int context, int i, ref MPMeshData md);
    [DllImport ("MassParticle")] public static extern int mpUpdateDataTexture(int context, IntPtr tex);
    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    public static extern int mpUpdateDataBuffer(int context, ComputeBuffer buf);

    [DllImport ("MassParticle")] public static extern int mpCreateContext();
    [DllImport ("MassParticle")] public static extern void mpDestroyContext(int context);
    [DllImport ("MassParticle")] public static extern void mpBeginUpdate(int context, float dt);
    [DllImport ("MassParticle")] public static extern void mpEndUpdate(int context);
    [DllImport ("MassParticle")] public static extern void mpUpdate (int context, float dt);
    [DllImport ("MassParticle")] public static extern void mpCallHandlers(int context);

    [DllImport ("MassParticle")] public static extern void mpClearParticles(int context);
    [DllImport ("MassParticle")] public static extern void mpClearCollidersAndForces(int context);
    [DllImport ("MassParticle")] public static extern MPKernelParams mpGetKernelParams(int context);
    [DllImport ("MassParticle")] public static extern void mpSetKernelParams(int context, ref MPKernelParams p);

    [DllImport ("MassParticle")] public static extern int mpGetNumParticles(int context);
    [DllImport ("MassParticle")] unsafe public static extern MPParticle* mpGetParticles(int context);
    [DllImport ("MassParticle")] unsafe public static extern void mpCopyParticles (int context, MPParticle *dst);
    [DllImport ("MassParticle")] unsafe public static extern void mpWriteParticles(int context, MPParticle *from);
    [DllImport ("MassParticle")] public static extern void mpScatterParticlesSphere(int context, ref Vector3 center, float radius, int num, ref MPSpawnParams sp);
    [DllImport ("MassParticle")] public static extern void mpScatterParticlesBox(int context, ref Vector3 center, ref Vector3 size, int num, ref MPSpawnParams sp);
    [DllImport ("MassParticle")] public static extern void mpScatterParticlesSphereTransform(int context, ref Matrix4x4 trans, int num, ref MPSpawnParams sp);
    [DllImport ("MassParticle")] public static extern void mpScatterParticlesBoxTransform(int context, ref Matrix4x4 trans, int num, ref MPSpawnParams sp);

    [DllImport ("MassParticle")] public static extern void mpAddSphereCollider(int context, ref MPColliderProperties props, ref Vector3 center, float radius);
    [DllImport ("MassParticle")] public static extern void mpAddCapsuleCollider(int context, ref MPColliderProperties props, ref Vector3 pos1, ref Vector3 pos2, float radius);
    [DllImport ("MassParticle")] public static extern void mpAddBoxCollider(int context, ref MPColliderProperties props, ref Matrix4x4 transform, ref Vector3 size);
    [DllImport ("MassParticle")] public static extern void mpRemoveCollider(int context, ref MPColliderProperties props);

    [DllImport ("MassParticle")] public static extern void mpAddForce (int context, ref MPForceProperties regionType, ref Matrix4x4 regionMat);

    [DllImport ("MassParticle")] public static extern void mpScanSphere (int context, MPHitHandler h, ref Vector3 center, float radius);
    [DllImport ("MassParticle")] public static extern void mpScanSphereParallel (int context, MPHitHandler h, ref Vector3 center, float radius);
    [DllImport ("MassParticle")] public static extern void mpScanAABB (int context, MPHitHandler h, ref Vector3 center, ref Vector3 extent);
    [DllImport ("MassParticle")] public static extern void mpScanAABBParallel (int context, MPHitHandler h, ref Vector3 center, ref Vector3 extent);
    [DllImport ("MassParticle")] public static extern void mpScanAll (int context, MPHitHandler h);
    [DllImport ("MassParticle")] public static extern void mpScanAllParallel (int context, MPHitHandler h);

    [DllImport ("MassParticle")] public static extern void mpMoveAll(int context, ref Vector3 move_amount);
}


public static class MPUtils
{
    public static void AddRadialSphereForce(MPWorld world, Vector3 pos, float radius, float strength)
    {
        Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * radius);
        MPForceProperties p = new MPForceProperties();
        p.shape_type = MPForceShape.Sphere;
        p.dir_type = MPForceDirection.Radial;
        p.radial_center = pos;
        p.strength_near = strength;
        p.strength_far = 0.0f;
        p.attenuation_exp = 0.5f;
        p.range_inner = 0.0f;
        p.range_outer = radius;
        world.AddOneTimeAction(() => {
            MPAPI.mpAddForce(world.GetContext(), ref p, ref mat);
        });
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IntFloatUnion
    {
    [FieldOffset(0)]  public int i32;
    [FieldOffset(0)]  public float f32;
    }

    public static int FloatToBinary(float f)
    {
        IntFloatUnion ifu;
        ifu.i32 = 0; // shut up compiler
        ifu.f32 = f;
        return ifu.i32;
    }

    public static float BinaryToFloat(int b)
    {
        IntFloatUnion ifu;
        ifu.f32 = 0.0f; // shut up compiler
        ifu.i32 = b;
        return ifu.f32;
    }
}
