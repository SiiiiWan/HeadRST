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
- `Eye Gaze`: reads, filters, and monitors the headset eye gaze ray.
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

## Study Controls

Select the `Control` object in the scene to access the study settings.

- `IsPractice`: enables the practice sequence when checked and the formal task sequence when unchecked.
- `Skip Current Trial`: available in Play Mode from the `StudyControl` inspector. It removes the current object and target so the task sequence advances to the next trial.

The task is started from the in-scene start button during Play Mode, not from a separate inspector start button.

Practice mode uses the practice depth/amplitude condition and shows a practice object before the task begins. After the practice sequence finishes, the start button is shown again so another run can be started.

Formal mode runs the full task condition set and shows the completion text after all trials are finished.

## Paper Parameters

The core tuning parameters are exposed in the inspector and aligned with the paper notation where they are intended for adjustment.

Technique parameters:

- `theta_thr = 15` on `Magic`, `MagicPitch`, and `MagicModPitch`
- `MinDepth = 1` on `MagicPitch` and `MagicModPitch`
- `MaxDepth = 11` on `MagicPitch` and `MagicModPitch`
- `v_min = 0.1` on `MagicPitch` and `MagicModPitch`
- `v_max = 0.6` on `MagicPitch` and `MagicModPitch`
- `G_min = 0` on `MagicPitch` and `MagicModPitch`
- `G_max = 0.8` on `MagicPitch` and `MagicModPitch`
- `v_hmax = 0.1` on `MagicModPitch`

Input integration parameters:

- `t_fixation = 0.25`
- `theta_fixation = 3`

Eye tracking health parameters on `Eye Gaze`:

- `HeadAlignedWarningAngle = 0.5`
- `HeadAlignedWarningDuration = 1.5`
- `HeadAlignedWarningRepeatInterval = 5`

The eye tracking health warning is triggered when the gaze direction stays nearly identical to the head direction for a sustained period. This usually indicates that headset eye tracking is unavailable, disabled, or not calibrated.

## Running The Demo

1. Open the project with Unity `6000.0.44f1`.
2. Open `Assets/Scenes/Sample Scene.unity`.
3. Connect a supported Meta XR headset.
4. Confirm eye tracking and hand tracking are enabled on the headset.
5. Select `Control` in the hierarchy.
6. Choose the technique and dominant hand in `TechniqueControl`.
7. Set `IsPractice` in `StudyControl`.
8. Enter Play Mode or build to the headset.
9. Press the in-scene start button to begin the task.
10. Select objects with gaze and pinch, then manipulate them according to the selected technique.

In practice mode, the scene first provides a practice object and then runs the practice task condition. When the practice task is complete, the start button returns so the demo can be repeated.

In formal mode, the scene runs the full formal task condition set. When all trials are complete, the completion text is shown.

During either mode, `Skip Current Trial` can be used from the `StudyControl` inspector in Play Mode to advance past the current trial.

## Troubleshooting

- If gaze input is null, check the headset eye tracking permission and the `Eye Gaze` object references.
- If gaze and head directions remain aligned and a warning appears, check headset eye tracking permission, calibration, and device support.
- If hand input is null, check hand tracking permission and the `Hand Tracking` references.
- If pinch feedback is missing, check the `PinchBalls` objects and the active dominant hand.
- If technique switching appears stuck, verify that only one of the four technique components is active through `TechniqueControl`.
- If scene references appear missing after an external edit, reload the scene in Unity before saving it again.
