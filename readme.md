# ITExpert OpenApi 

Набор CLI инструментов для работы с OpenAPI спецификациями (Swagger).

Инструментарий призван облегчить работу с REST API уменьшая зависимость от удаленного сервера в т.ч. включая полностью автономную работу (оффлайн).
Это осуществляется за счет использования подробных спецификаций серверных методов, которые должны быть доступны с машины пользователя. 

## Функционал

* SwaggerUI - отображение докуентации API.
* MockServer - встроенный HTTP-сервер, принимающий запросы от клиента и генерирющий ответ по спецификации.
* Proxy - встроенный HTTP-сервер также может перенаправлять запросы на удаленный сервер.
* Валидация запросов и ответов на соответствие спецификации.
* Работа с файлами спецификации (загрузка, объединение и т.п.).

## Установка

### dotnet tool

> Для установки через `dotnet tool` необходим [.net sdk 2.1+](https://www.microsoft.com/net/download/dotnet-core/2.1).

1. `dotnet tool install -g itexpert.openapi`
2. `oas --version`

### binaries

* [win-x64](https://git.itexpert.ru/ITExpert/ITExpert.OpenApi/-/jobs/artifacts/master/download?job=win-x64)
* [osx-x64](https://git.itexpert.ru/ITExpert/ITExpert.OpenApi/-/jobs/artifacts/master/download?job=osx-x64)
* [linux-x64](https://git.itexpert.ru/ITExpert/ITExpert.OpenApi/-/jobs/artifacts/master/download?job=linux-x64)

1. Скачать и распаковать архив
2. [Опцинально] Добавить путь до распакованной директории в `PATH`
3. `oas --version`

### docker

`docker run -p 5000:80 -v .oas:/app/.oas itexpert/openapi run`

В этом случае пердполагается, что рабочая директория выгляит следующим образом:

```
oas/
├── spec/
|   ├── example_spec_1.yml
|   └── example_spec_2.yaml
|   └── example_spec_3.json
└── openapi.config.json

```

## Использование

> см. oas --help