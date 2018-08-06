# ITExpert OpenApi 

Набор CLI инструментов для работы с OpenAPI спецификациями (Swagger).

Инструментарий призван облегчить работу с REST API уменьшая зависимость от 
удаленного сервера в т.ч. включая полностью автономную работу (оффлайн).
Это осуществляется за счет использования подробных спецификаций серверных 
методов, которые должны быть доступны с машины пользователя. 

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
2. [*Опцинально*] Добавить путь до распакованной директории в `PATH`
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
└── oas.config.json

```

## Использование

> см. oas --help

## Конфигруация сервера

Для конфигурации сервера используется JSON-файл (по умолчанию `.oas/oas.config.json`),
путь до которого может быть задан через ключ `-c|--config` команды `run`.

### Параметры конфига

* `mockServerHost` - адрес до MockServer, используемый в SwaggerUI. 
  По умолчанию вычисляется как `http:localhost:{Port}`, 
  где `Port` является TCP-портом, который слушает сервер.
* `routes`: (`Array<Object>`) - настройки эндпоинтов загруженных на сервер спецификаций.
    *  `path`: (`Regexp`) - регулярное выражение, сопоставляемое с каждым путем 
       из спецификации.
    *  `method`: (`any | get | post | put | delete | patch | head | options | trace | connect`) - 
       HTTP-метод эндпоинта спецификации.
    *  `validate`: (`none | request | response | all`) - режим валидации запроса \ 
       ответа, проходящих через сервер, где 
       `none` - без валидации, 
       `all` - валидация запроса и ответа, 
       `request` и `response` - валидация только запроса или ответа, соответственно.
    *  `mock`: (`Boolean`) - `true`, если на обращение к эндпоинту нужно 
       сгенерировать ответ вместо обращения к удаленному серверу.
    *  `delay`: (`Integer`) - минимальное время выполнения запроса в миллисекундах.

Правила конфига накладываются друг на друга. При этом "верхние" (с меньшим индексом в массиве),
переопределяют "нижние", поэтому порядок имеет значение.

По умолчанию используется удаленный сервер (`mock: false`), без валидации (`validate: "none"`)
и задержки (`delay: 0`).

Если какое-то свойство правила не задано, берется значение свойства по умолчанию.

### Пример конфига

```json
{
    "mockServerHost": "http://localhost:35200",
    "routes": [
        {
            "path": "/api/",
            "method": "any",
            "delay": 100,
            "validate": "all",
            "mock": true
        },
        {
            "path": ".*",
            "method": "any",
            "validate": "none",
            "mock": false,
            "delay": 0
        }
    ]
}
```
