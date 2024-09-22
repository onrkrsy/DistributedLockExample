# Net 8 ve Redis ile Distrubuted Lock 

### Açıklama
Bu proje, Redis kullanarak .NET Core üzerinde dağıtık kilitleme (distributed locking) mekanizmasını uygular. Kilit süresinin dolmasından önce işlemin tamamlanmaması durumunda, kilit yenileme mekanizmasıyla kilit periyodik olarak uzatılır. Bu sayede, veri tutarlılığı korunur ve kilit süresi dolmadan işlem güvenli bir şekilde tamamlanır.

Projenin detaylı anlatımını içeren makaleye aşağıdaki linkten ulaşabilirsiniz:

[**.NET Core'da Redis ile Distributed Locking Kullanımı**](https://medium.com/@onurkarasoy/net-coreda-redis-ile-distributed-locking-kullan%C4%B1m%C4%B1-95a70afdeb4e)

 

---
### Kullanılan Teknolojiler
- .NET Core 8
- Redis
- StackExchange.Redis
- Docker (Redis ve servislerin ayağa kaldırılması için)

### Kurulum

**Proje Deposu:** 
   Projeyi yerel ortamınıza klonlayın:
   ```bash
   git clone https://github.com/kullaniciadi/redis-locking-example.git
   ```

**Docker Compose ile Servisleri Çalıştırma**
Docker Compose kullanarak Redis, Redis Commander ve StockService'i ayağa kaldırmak için:

  ```bash
docker-compose up --build
   ```

### Kullanım

Proje içinde `StockController` sınıfı ile ürünlerin stoklarını güncelleyebilirsiniz. Kilit süresi boyunca işlem tamamlanmazsa, kilit yenilenir.


- **Stok Güncelleme Endpoint:**

  **PUT** `/update-stock/{productId}`

  Body parametresi:
  ```json
  {
    "quantity": 10
  }
  ```

  Bu endpoint, belirli bir `productId` için kilit alır ve stok güncelleme işlemi yapar. Eğer işlem süresi 30 saniyeyi aşarsa, kilit yenilenir ve işlem tamamlanana kadar kilitli kalır.

### Kilit Yenileme Mekanizması

Bu proje, kilit süresi dolmadan önce kilidin periyodik olarak yenilenmesini sağlar. `RedisLockService` sınıfı içinde `RenewLockAsync` metodu kullanılarak kilit her 15 saniyede bir yenilenir. Böylece işlem devam ederken kilidin geçerliliği korunur.

```csharp
public async Task RenewLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(expiry / 2); // Expiry süresinin yarısında kilidi yenile
        await _database.LockExtendAsync(key, Environment.MachineName, expiry);
    }
}
```

### Örnek Kullanım

```bash
PUT http://localhost:5000/update-stock/1
{
  "quantity": 10
}
```

Eğer kilit alınabilirse, stok başarıyla güncellenir. Eğer kilit alınamazsa, **429 Too Many Requests** hata kodu döner.

 

---
Projenin detaylı anlatımını içeren makaleye aşağıdaki linkten ulaşabilirsiniz:

[**.NET Core'da Redis ile Distributed Locking Kullanımı**](https://medium.com/@onurkarasoy/net-coreda-redis-ile-distributed-locking-kullan%C4%B1m%C4%B1-95a70afdeb4e)
 
