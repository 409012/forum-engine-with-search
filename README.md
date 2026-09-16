# FEwS — Forum Engine with Search

FEwS — это современный микросервисный движок форумов с поддержкой полнотекстового поиска, событийной индексации и мониторинга. Проект построен на .NET 9 и демонстрирует современные подходы к архитектуре, масштабируемости и наблюдаемости.

## Архитектура

FEwS состоит из двух основных микросервисов:
- **Forums** — сервис управления форумами, темами и сообщениями.
- **Search** — сервис полнотекстового поиска по форумам и их содержимому.

Между сервисами реализовано событийное взаимодействие через Kafka: все изменения в форумах публикуются как события, которые индексируются сервисом поиска.

## Технологии и стек

- **.NET 9, ASP.NET Core** — основа для API и сервисов.
- **Entity Framework Core + PostgreSQL** — хранение данных форумов, миграции, работа с БД.
- **OpenSearch** — полнотекстовый поиск и хранение поискового индекса.
- **Kafka (Confluent.Kafka)** — event-driven взаимодействие между сервисами, асинхронная индексация.
- **gRPC** — высокопроизводительное межсервисное взаимодействие для поиска.
- **MediatR** — реализация паттерна CQRS, обработка команд и запросов.
- **FluentValidation** — валидация входных данных.
- **AutoMapper** — маппинг между слоями и моделями.
- **OpenTelemetry + Jaeger + Prometheus + Grafana** — трассировка, метрики, мониторинг, визуализация.
- **Serilog + Loki** — централизованный сбор и просмотр логов.
- **Docker Compose** — контейнеризация и локальный запуск всей инфраструктуры.

## Как запустить

1. Установите Docker и Docker Compose.
2. В корне репозитория запустите инфраструктуру:
   ```sh
   docker compose -f docker/docker-compose.yml up -d postgres kafka-broker opensearch grafana jaeger loki prometheus opensearch-dashboard
   ```
3. Примените миграции до первого запуска CDC-коннектора. Команда требует установленного инструмента `dotnet-ef` версии 9:
   ```sh
   dotnet ef database update --project src/FEwS.Forums.Storage --startup-project src/FEwS.Forums.API
   ```

4. Соберите и запустите коннектор:
   ```sh
   docker compose -f docker/docker-compose.yml up -d --build kafka-connect kafka-ui
   ```

5. Соберите и запустите микросервисы (Forums, Search, ForumConsumer) через dotnet CLI или вашу IDE.

6. Откройте Swagger UI для тестирования API:
   - Forums: http://localhost:5000/swagger
   - Search: https://localhost:5002/swagger

7. Для мониторинга и логирования доступны:
   - Grafana: http://localhost:3000
   - Jaeger: http://localhost:16686
   - Prometheus: http://localhost:9090
   - OpenSearch Dashboard: http://localhost:5601
   - Kafka UI : http://localhost:8082

## Доставка событий из outbox

Событие сохраняется в `DomainEvents` в одной транзакции с темой или комментарием. Debezium читает подтверждённые изменения из WAL PostgreSQL через `pgoutput`. Поле `EmittedAt` не используется как позиция чтения: одинаковые timestamps и транзакции, завершившиеся в другом порядке, не приводят к пропуску записей.
При первом запуске коннектор делает snapshot существующих записей, затем продолжает чтение WAL. Таблица `public."DomainEvents"` должна существовать до этого запуска. Коннектор создаёт publication и replication slot `fews_domain_events`. PostgreSQL запускается с `wal_level=logical`.
Формат для consumer сохранён: topic `fews.DomainEvents`, ключ `DomainEventId`, JSON с Base64-полем `ContentBlob` и заголовок `activity_id`. Удаление строки из outbox не публикуется как удаление форумной сущности.
Позиция чтения хранится в volume `fews-connect`, а replication slot — в PostgreSQL. При пересоздании контейнера коннектор продолжает с сохранённой позиции. Не удаляйте volume с offsets или replication slot при обычном перезапуске. Доставка допускает повторы после сбоя; индексация использует стабильный идентификатор документа.

### Переход с JDBC-коннектора

Остановите старый коннектор перед переключением и пересоздайте PostgreSQL с новой конфигурацией. Данные в существующем volume PostgreSQL сохраняются:

```sh
docker compose -f docker/docker-compose.yml stop kafka-connect
docker compose -f docker/docker-compose.yml up -d postgres
dotnet ef database update --project src/FEwS.Forums.Storage --startup-project src/FEwS.Forums.API
docker compose -f docker/docker-compose.yml up -d --build kafka-connect kafka-ui
```

Проверьте состояние коннектора и его task:

```sh
curl --fail http://localhost:8083/connectors/fews.domain-events.cdc/status
```

Оба состояния должны быть `RUNNING`. Старый JDBC offset не переносится: первый snapshot повторно отправит сохранившиеся события, включая ранее пропущенные. Старые документы OpenSearch со случайными `_id` этим не удаляются.

Пока коннектор недоступен, replication slot удерживает необходимые WAL-сегменты. Контролируйте свободное место PostgreSQL и отставание коннектора; удаление slot или потеря WAL требует отдельного восстановления, а не обычного перезапуска. Описание snapshot и продолжения чтения: [Debezium PostgreSQL connector](https://debezium.io/documentation/reference/2.7/connectors/postgresql.html).

## Возможности

- Создание и просмотр форумов и тем.
- Асинхронная индексация новых тем и комментариев.
- Быстрый полнотекстовый поиск по форумам.
- Расширенный мониторинг, трассировка и централизованные логи.
- Масштабируемая событийная архитектура.

## Для чего этот проект

FEwS — pet project для демонстрации:
- Микросервисной архитектуры на .NET
- Event-driven подхода с Kafka
- Интеграции с современными инструментами поиска и мониторинга
- Применения best practices (CQRS, DI, валидация, контейнеризация)
