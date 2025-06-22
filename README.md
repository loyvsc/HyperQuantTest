[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-1384D5)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

# Тестовое задание: HyperQuantTest

## 📌 Цель
Разработать **универсальный коннектор** к бирже Bitfinex, который:
1. Стандартизирует работу с REST и WebSocket API
2. Реализует ключевые функции для трейдинга и аналитики
3. Демонстрирует навыки работы с API и C#

## 📊 Архитектура
### Layered Architecture (Многослойная архитектура)

```mermaid
graph TD
    A[Presentation Layer] --> B[Data Access Layer]
    B --> C[Application Core]
