# MagicPitch Digital Appendix

This Unity project contains the demo scene for **MagicPitch: 3D Object Manipulation with Gaze, Pinch, and Head Pitch**. It is prepared as a digital appendix with the four techniques described in the paper:

- `GazePinch`
- `Magic`
- `MagicPitch`
- `MagicModPitch`

The project is intended to demonstrate the manipulation techniques and the task scene. Study data logging has been removed from this appendix build.

## Environment

- Unity: `6000.0.44f1`
- Render pipeline: Universal Render Pipeline
- Meta XR SDK Core: `76.0.1`
- Meta XR Interaction SDK OVR: `76.0.1`
- XR Plug-in: Oculus XR Plug-in `4.5.1`

Use a Meta XR headset and runtime setup that supports eye tracking and hand tracking. Enable eye tracking and hand tracking permissions on the device before running the scene.

## Scene

Open:

```text
Assets/Scenes/Sample Scene.unity
```

The scene contains the task environment, target objects, gaze input, hand input, pinch visualization, and the technique control object needed for the demo.

## Main Objects

- `Control`: central study/demo control object.
- `TechniqueControl`: selects the active technique and dominant hand.
- `TechniqueInputProvider`: combines eye, head, and hand input into a shared input layer for the techniques.
- `Eye Gaze`: reads and filters the headset eye gaze ray.
- `Hand Tracking`: provides hand pose and pinch input.
- `PinchBalls`: visualizes pinch positions.

## Switching Techniques

Select the `Control` object in the scene and use the `TechniqueControl` inspector.

Available technique modes:

```text
GazePinch
Magic
MagicPitch
MagicModPitch
```

Set `DominantHand` to `left` or `right` before running the demo. The selected technique is activated through the inspector control so only one technique is active at a time.

## Paper Parameters

The core tuning parameters are exposed in the inspector and aligned with the paper notation where they are intended for adjustment.

Technique parameters:

- `MinDepth = 1`
- `MaxDepth = 11`
- `theta_thr = 15`
- `v_min = 0.1`
- `v_max = 0.6`
- `G_min = 0`
- `G_max = 0.8`
- `v_hmax = 0.1` for `MagicModPitch`

Input integration parameters:

- `t_fixation = 0.25`
- `theta_fixation = 3`

## Running The Demo

1. Open the project with Unity `6000.0.44f1`.
2. Open `Assets/Scenes/Sample Scene.unity`.
3. Connect a supported Meta XR headset.
4. Confirm eye tracking and hand tracking are enabled on the headset.
5. Select `Control` in the hierarchy.
6. Choose the technique and dominant hand in `TechniqueControl`.
7. Enter Play Mode or build to the headset.
8. Select objects with gaze and pinch, then manipulate them according to the selected technique.

## Troubleshooting

- If gaze input is null, check the headset eye tracking permission and the `Eye Gaze` object references.
- If hand input is null, check hand tracking permission and the `Hand Tracking` references.
- If pinch feedback is missing, check the `PinchBalls` objects and the active dominant hand.
- If technique switching appears stuck, verify that only one of the four technique components is active through `TechniqueControl`.
- If scene references appear missing after an external edit, reload the scene in Unity before saving it again.
