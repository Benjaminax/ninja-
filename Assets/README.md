# The Paradox - 2D Pixel Adventure | Technical Portfolio

**The Paradox** là một dự án game 2D Platformer được phát triển nhằm mục đích nghiên cứu và áp dụng các kỹ thuật lập trình Game hiện đại trong Unity. Dự án tập trung vào việc tối ưu hóa trải nghiệm người dùng (Game Feel), xây dựng hệ thống AI linh hoạt và ứng dụng workflow UI Toolkit tiên tiến.

---

## 1. Tổng quan kỹ thuật (Technical Overview)
*   **Engine:** Unity 6 (6000.3.10f1) - Tận dụng các tính năng mới nhất về hiệu suất.
*   **Render Pipeline:** Universal Render Pipeline (URP) - Cấu hình 2D Renderer giúp tối ưu ánh sáng và hậu kỳ.
*   **Input System:** New Input System (Package) - Xử lý input theo hướng sự kiện (Event-driven), hỗ trợ đa thiết bị.
*   **Kiến trúc mã nguồn:** 
    *   **OOP (Inheritance):** Sử dụng lớp cơ sở `Enemy.cs` để đóng gói logic chung (Máu, Sát thương, Nhận sát thương, Chết). Các class con như `ChickenAI`, `TurtleAI` kế thừa và mở rộng hành vi riêng biệt.
    *   **Singleton Pattern:** Áp dụng cho `GameData` để quản lý trạng thái điểm số và dữ liệu xuyên suốt các màn chơi.
    *   **Observer Pattern:** Sử dụng C# Events để thông báo thay đổi dữ liệu từ hệ thống lên UI mà không gây phụ thuộc chéo (Decoupling).

---

## 2. Chi tiết triển khai kỹ thuật (Technical Implementation)

### 2.1. Cơ chế di chuyển nâng cao (Game Feel)
*   **Coyote Time & Jump Buffer:** Triển khai trong `PlayMove.cs` để xử lý các lệnh nhảy "nhân văn" hơn.
    *   *Cách áp dụng:* Sử dụng các biến đếm `coyoteTimeCounter` (cho phép nhảy sau khi rời khỏi cạnh một khoảng ngắn) và `jumpBufferCounter` (ghi nhớ lệnh nhảy khi nhân vật chưa chạm đất).
    *   *Hiệu quả:* Cải thiện đáng kể độ nhạy và cảm giác điều khiển, giúp gameplay trở nên mượt mà, chuyên nghiệp.
*   **Double Jump Logic:** Tích hợp bộ đếm `jumpCount` kết hợp với hệ thống Animator để kích hoạt các trạng thái nhảy đôi riêng biệt.

### 2.2. Hệ thống AI & Nhận thức môi trường

Hệ thống trí tuệ nhân tạo (AI) của kẻ thù trong **The Paradox** được xây dựng theo kiến trúc hướng module, kết hợp linh hoạt giữa máy trạng thái (State Machine) và các kỹ thuật nhận thức vật lý để tối ưu hiệu suất đồng thời tạo ra các phản xạ tự nhiên, sinh động.

#### a. Các cơ chế nhận thức cốt lõi (Core Mechanics)
* **Raycasting Detection:** Sử dụng `Physics2D.Raycast` để quét môi trường theo thời gian thực. AI bắn các tia xuống dưới (kiểm tra vực thẳm) và phía trước (kiểm tra tường), giúp chúng tự động quay đầu (`Flip`) hoặc dừng lại thay vì di chuyển vô hồn.
* **Position & Distance Detection:** Liên tục tính toán khoảng cách và vị trí tương đối (`xDistance`, `yDistance`) giữa AI và Player để kích hoạt các trạng thái phản kháng, đuổi theo hoặc tấn công bất ngờ.
* **Timer-Based State Machine:** Quản lý vòng đời hành vi thông qua các bộ đếm thời gian (`stateTimer`), giúp quái vật chuyển đổi mượt mà giữa các trạng thái nghỉ ngơi và tuần tra, tạo nhịp độ tự nhiên cho màn chơi.
* **Asynchronous Simulation:** Sử dụng logic khởi tạo ngẫu nhiên hướng di chuyển và bộ đếm thời gian (`stateTimer`) trong hàm `Start()`. Kỹ thuật này giúp các kẻ thù hoạt động không đồng bộ, tránh hiện tượng di chuyển rập khuôn, mang lại cảm giác thế giới game sinh động và tự nhiên hơn.
* **Dynamic Hit Detection:** Phân tích hướng va chạm dựa trên Vector Normal của `Collision2D` để quyết định logic: Người chơi tiêu diệt quái vật (va chạm từ trên xuống - *Stomp*) hoặc Người chơi nhận sát thương (va chạm theo phương ngang/dưới lên).

#### b. Chi tiết thiết kế AI của các Archetype Kẻ thù

##### 1. Mushroom (Nấm) - Hybrid AI
* **Cơ chế:** *Timer-Based State Machine* kết hợp *Raycasting Detection*.
* **Cách hoạt động:** Sử dụng bộ đếm `stateTimer` để chuyển đổi qua lại giữa trạng thái Nghỉ (Idle) và Di chuyển (Walk). Khi đang di chuyển, `Physics2D.Raycast` liên tục kiểm tra bề mặt phía trước. Nếu phát hiện tường hoặc rìa vực, quái sẽ tự động đổi hướng di chuyển.
* **Mục đích:** Kẻ thù tuần tra tự động trên các nền tảng (Platform), biết tự bảo vệ mình không bị rơi khỏi địa hình.

##### 2. BlueBird (Chim xanh) - Coordinate-Based AI
* **Cơ chế:** *Ping-pong Patrol* (Tuần tra dựa trên tọa độ).
* **Cách hoạt động:** Lưu vị trí bắt đầu (`startPosition`) làm tâm. Kẻ thù di chuyển liên tục sang trái và phải trong một phạm vi cố định (`patrolRange`) bằng cách tính toán khoảng cách toán học so với điểm xuất phát để quyết định thời điểm quay đầu.
* **Mục đích:** Tạo ra chướng ngại vật di động trên không với quỹ đạo cố định, thử thách khả năng căn thời gian (Timing) của người chơi.

##### 3. Chicken (Gà) - Reactive AI
* **Cơ chế:** *Proximity Detection* (Phát hiện khoảng cách gần) kết hợp *Raycasting*.
* **Cách hoạt động:** Liên tục quét khoảng cách với Player. Khi người chơi bước vào vùng phát hiện (`detectionRange`), AI lập tức chuyển sang trạng thái Đuổi theo (`isChasing`) với tốc độ cao. Trong quá trình truy đuổi, tia `Raycast` vẫn hoạt động để ép quái dừng lại nếu gặp vực sâu, tránh bị người chơi "dụ" tự sát.
* **Mục đích:** Kẻ thù mang tính chủ động cao, tạo áp lực tâm lý và buộc người chơi phải xử lý nhanh khi tiếp cận.

##### 4. FatBird (Chim béo) - Trigger-Based AI
* **Cơ chế:** *Sinusoidal Movement* (Chuyển động hình Sin) kết hợp *Position Check*.
* **Cách hoạt động:** * *Trạng thái tuần tra:* Bay lơ lửng tại chỗ bằng cách áp dụng hàm toán học `Mathf.Sin` vào trục Y để tạo cảm giác mượt mà.
    * *Trạng thái tấn công:* So sánh tọa độ để phát hiện nếu người chơi đi vào vùng ngay bên dưới chân. Khi đủ điều kiện, AI chuyển cấu hình Rigidbody2D sang `Dynamic` để rơi tự do tốc độ cao xuống nghiền nát mục tiêu.
* **Mục đích:** Đóng vai trò như một chiếc bẫy bất ngờ từ trên cao, buộc người chơi phải cẩn trọng quan sát không gian phía trên trước khi di chuyển.

##### 5. Turtle (Rùa) - Simple State AI
* **Cơ chế:** *Timer-Based State Machine* đơn giản.
* **Cách hoạt động:** Hoạt động với logic tối giản dựa trên bộ đếm thời gian để đứng yên và bò chậm rãi. Không sử dụng các tia quét môi trường phức tạp nhằm tối ưu hóa hiệu năng cho các thực thể số lượng lớn.
* **Mục đích:** Kẻ thù cơ bản tại các màn chơi mở đầu, giúp người chơi dễ dàng làm quen với cơ chế tấn công nhảy đạp (Stomp).

### 2.3. Hệ thống giao diện hiện đại (UI Toolkit)
*   **UXML & USS Workflow:** Thay thế uGUI truyền thống bằng UI Toolkit để xây dựng HUD.
    *   *Cách áp dụng:* Thiết kế layout bằng `HUD.uxml` và định dạng phong cách bằng `HUD.uss` (tư duy tương tự Web Flexbox).
    *   *Hiệu quả:* Tách biệt hoàn toàn logic (C#) và giao diện (XML/CSS), giúp dễ dàng thay đổi skin game mà không cần can thiệp vào code hệ thống.

### 2.4. Level Design với RuleTiles & Tilemap
*   **Hệ thống RuleTiles thông minh:** Ứng dụng `RuleTile` (ScriptableObject) để tự động hóa quy trình vẽ địa hình phức tạp.
    *   *Cách áp dụng:* Thiết lập các tập luật (Rules) cho từng loại gạch (Green, Orange, Stone...). Khi vẽ lên Tilemap, hệ thống tự động nhận diện các ô lân cận để chọn Sprite phù hợp (góc, cạnh, bề mặt).
    *   *Hiệu quả:* Tăng tốc độ thiết kế màn chơi lên gấp nhiều lần, đảm bảo môi trường game luôn liền mạch, đúng cấu trúc thẩm mỹ mà không cần đặt từng viên gạch thủ công.

---

## 3. Tối ưu hóa & Quy trình tài nguyên

### 3.1. Resource Optimization
*   **Prefab Workflow:** Mọi đối tượng (Player, Enemy, Item) đều được đóng gói thành Prefab độc lập. Giúp quản lý cập nhật đồng bộ và hỗ trợ tái sử dụng tài nguyên hiệu quả.
*   **Pixel Art Consistency:** 
    *   *Thiết lập:* Toàn bộ Sprite được cấu hình `Filter Mode: Point` và `Compression: None`.
    *   *Hiệu quả:* Giữ nguyên độ sắc nét của từng pixel, không bị mờ nhòe trên các độ phân giải màn hình khác nhau. Sử dụng **Pixel Perfect Camera** để loại bỏ hiện tượng rung pixel khi nhân vật di chuyển.

### 3.2. Quy trình Git Flow (Project Timeline)
Dự án được triển khai qua các giai đoạn Commit chuẩn hóa:
1.  **Initialize:** Cấu hình `.gitignore` chuẩn Unity và khởi tạo cấu trúc folder.
2.  **Environment Setup:** Thiết lập URP, Layers (Ground, Player, Enemy) và Tags.
3.  **Core Gameplay:** Hoàn thiện `PlayerController` với hệ thống New Input System.
4.  **AI Implementation:** Xây dựng Base Class `Enemy` và các loại AI cụ thể (Chicken, Turtle, Bird).
5.  **UI Integration:** Tích hợp UI Toolkit và hệ thống Event-driven data binding.
6.  **Tooling:** Xây dựng các RuleTiles cho môi trường.
7.  **Final Polish:** Tối ưu hóa hiệu suất và đóng gói tài liệu kỹ thuật.

---

### Liên hệ & Portfolio
*   **Developer:** Nguyễn Hữu Sang
*   **Vị trí ứng tuyển:** Unity Developer Intern
*   **Project Status:** Hoàn thiện các tính năng cốt lõi.
