using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnywhereHandContinuous : ManipulationTechnique
{

    public override void ApplySingleHandGrabbedBehaviour()
    {



        GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
    }

}