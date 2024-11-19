## Sprite Frame Documentation

### **Frame Direction Mapping**
Each frame section consists of 5 directions, with the positions within the section mapping to specific directions:

- **Frame (Position) + 0**: **Up**  
- **Frame (Position) + 1**: **Diagonal Right**  
- **Frame (Position) + 2**: **Right**  
- **Frame (Position) + 3**: **Diagonal Left**  
- **Frame (Position) + 4**: **Down**  

---

### **Frame Categories**
Animations are divided into sections, where each section corresponds to a specific action. Each section spans multiple groups of 5 frames (one group per direction):

#### **Idle Animation**
- **Frames 0–4**: Unit is idle.  
  - Directions: **Up**, **Diagonal Right**, **Right**, **Diagonal Left**, **Down**.

#### **Movement Animation**
- **Frames 5–9**: Initial movement animation.  
  - Directions: **Up**, **Diagonal Right**, **Right**, **Diagonal Left**, **Down**.

#### **Attack Animations**
1. **Attack Start**:  
   - **Frames 10–24**: Beginning phase of an attack.  
     - Frames are grouped in sets of 5 for each direction.
2. **Attack Motion**:  
   - **Frames 30–34**: Core attack motion.  
     - One group of frames for all directions.
3. **Final Attack**:  
   - **Frames 40–49**: Concluding phase of an attack.  
     - Frames grouped in sets of 5 for each direction.

#### **Death Animations**
1. **Death Start**:  
   - **Frames 25–29**: Beginning of the death sequence.  
     - Frames grouped in sets of 5 for each direction.
2. **Death Final**:  
   - **Frames 35–39**: Final motion of the death sequence.  
     - One group of frames for all directions.

---

### **Usage Notes**
- **Directional Calculation**: For any frame section, add the direction offset (`+0` for Up, `+1` for Diagonal Right, etc.) to the base frame for the action:
  - Example Calculations:  
    - **Idle Up** → Frame `0 + 0 = 0`  
    - **Move Diagonal Left** → Frame `5 + 3 = 8`  
    - **Attack Start Down** → Frame `10 + 4 = 14`

- **Frame Groups by Action**: Actions like movement or attack are organized into contiguous blocks of frame groups, each spanning 5 frames for directions.

---

### **Frame Overview Table**

| **Action**        | **Frame Range** | **Frame Group**  | **Description**               |
|--------------------|-----------------|------------------|--------------------------------|
| **Idle**          | 0–4             | `0 + Direction`  | Unit standing idle.            |
| **Move Start**    | 5–9             | `5 + Direction`  | Initial movement animation.    |
| **Attack Start**  | 10–24           | `10 + Direction` | Starting phase of an attack.   |
| **Death Start**   | 25–29           | `25 + Direction` | Beginning of the death sequence. |
| **Attack Motion** | 30–34           | `30 + Direction` | Attack in progress.            |
| **Death Final**   | 35–39           | `35 + Direction` | End of the death sequence.     |
| **Final Attack**  | 40–49           | `40 + Direction` | Concluding phase of an attack. |