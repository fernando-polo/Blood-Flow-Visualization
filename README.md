# Blood Flow Visualization – Congenital Heart Disease Demo 💉

An interactive XR application built with Unity for the immersive visualization of blood flow in 3D vascular models, developed during an academic internship at the École de technologie supérieure (ÉTS) in Montréal, Canada.

## 📌 Description
This project is an interactive demo application designed to visualize and compare blood flow behavior in three-dimensional models of the aorta and iliac arteries under different physiological conditions.

It simulates hemodynamic parameters such as blood flow rate, pressure gradient, and turbulence using real hemodynamic formulas implemented through C# scripts and Unity Shader Graph. The application was developed as an educational tool for students and researchers in anatomy, medicine, and biomedical simulation.

The project was carried out at the Interventional Imaging Laboratory (LIVE) of ÉTS, in collaboration with the CHU Sainte-Justine University Hospital, as part of a broader initiative to integrate immersive technologies into medical education and vascular research.

## 📦 Tech Stack
**3D Engine:** Unity (URP, Shader Graph, C#)
**3D Modeling & Optimization:** Blender
**Medical Data & Simulation:** SimVascular, ParaView
**XR Platform:** Apple Vision Pro (VisionOS)
**Build & Export:** Xcode, PolySpatialKit
**Tools:** Git, Visual Studio

## 🫆 Features
- Three comparative physiological scenarios: healthy aorta, mild stenosis, and severe stenosis
- Real-time simulation of blood flow rate (L/min) using the Hagen-Poiseuille law
- Pressure gradient visualization with dynamic color mapping via custom Shader Graph
- Turbulence simulation based on Reynolds number calculation
- Interactive UI with scene navigation between scenarios and a real-time flow bar indicator
- Custom materials and shaders: BloodFlow, VesselWall, PressureGradient, Turbulence
- Successfully exported and validated on Apple Vision Pro

## ⚙️ How it works
The application follows a pipeline that starts from medical imaging data and ends in an immersive XR experience.

- A 3D aorta model was obtained from SimVascular using a public dataset and converted from `.vtp` to `.stl` via ParaView
- The model was optimized in Blender (polygon reduction, mesh correction, material preparation) and imported into Unity
- Three physiological scenarios were created by duplicating the model and adjusting C# script parameters to reflect different clinical states
- Hemodynamic parameters are calculated at runtime using simplified hydraulic-electric analogy models: Poiseuille's law for flow and resistance, pressure gradient as ΔP/L, and the Reynolds number for turbulence estimation
- Computed values are passed as shader properties (`_FlowSpeed`, `_PressureGradient`, `_Reynolds`) to drive the visual representation of each parameter on the 3D model
- A Unity Canvas UI allows the user to navigate between scenarios and monitor a live flow bar
- The application was configured as an XR build and exported to Apple Vision Pro via Xcode, where compatibility and performance were validated

## 🚀 Project Scope
This project was developed as an end-to-end biomedical visualization prototype, covering:

- Medical 3D model processing and optimization pipeline (SimVascular → ParaView → Blender → Unity)
- Hemodynamic simulation using real physiological formulas adapted for real-time rendering
- Custom shader and material development for scientific visualization
- Interactive UI design and XR integration for Apple Vision Pro
- Educational validation of the prototype as a learning tool for cardiovascular physiology

It demonstrates how biomedical engineering, computational simulation, and immersive technologies can be combined to create accessible and scientifically grounded educational experiences.

## 📚 What I learned
- End-to-end pipeline for medical 3D model processing and optimization
- Hemodynamic simulation using Poiseuille's law, pressure gradient, and Reynolds number
- Custom shader development with Unity Shader Graph for scientific data visualization
- XR application development and deployment for Apple Vision Pro
- Balancing scientific fidelity with real-time performance constraints in immersive environments
- Interdisciplinary collaboration between computer science, biomedical engineering, and medicine
- Working in an applied research laboratory context with real academic and clinical stakeholders


## 🎞️ Preview
<img width="789" height="492" alt="Image" src="https://github.com/user-attachments/assets/97519c8c-c6ef-4a42-89fb-20293cbfe659" />
