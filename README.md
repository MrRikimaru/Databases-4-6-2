## Предмет: Базы данных

## Задание: Курсовая работа

## Тема: Информационная система магазина автозапчастей

Магазин розничной торговли осуществляет заказ запчастей в различных странах. Ведется статистика продаж, отражающая спрос на те или иные детали, и, соответственно, потребность магазина в них (сколько единиц, на какую сумму, какого товара продано за последнее время) и на ее основе составляются заказы на требуемые товары. Выбор поставщика на каждый конкретный заказ осуществляют менеджеры магазина. В заказах перечисляется наименование товара, количество. Если указанное наименование товара ранее не поставлялось, оно пополняет справочник номенклатуры товаров.
Поставщики бывают различных категорий: фирмы, непосредственно производящие детали, дилеры, небольшие производства, мелкие поставщики и магазины. В результате поставщики различных категорий имеют различающийся набор атрибутов. Фирмы и дилеры - это самые надежные партнеры, они могут предложить полный пакет документов, скидки, а главное - гарантию, чего не может сделать небольшое производство или мелкий магазин. У них же (фирмы и дилеры) закупается большой объем продукции. Небольшое производство - это низкие цены, но никакой гарантии качества. В мелких лавках можно выгодно купить небольшое количество простых деталей, на которых сразу виден брак. Фирмы и дилеры поставляют детали на основе договоров, чего не делается для небольшого производства и мелкого магазина. В ходе маркетинговых работ изучается рынок поставщиков, в результате чего могут появляться новые поставщики и исчезать старые.
Когда ожидаются новые поставки, магазин собирает заявки от покупателей на свои товары. Груз приходит, производится его таможенное оформление, оплата пошлин, после чего он доставляется на склад в магазин. В первую очередь удовлетворяются заявки покупателей, а оставшийся товар продается в розницу.
В любой момент можно получить любую информацию о деталях, находящихся на складе, либо о поставляемых деталях. Детали хранятся на складе в определенных ячейках. Все ячейки пронумерованы. Касса занимается приемом денег от покупателей за товар, а так же производит возврат денег за брак. Брак, если это возможно, возвращается поставщику, который производит замену бракованной детали. Информация о браке (поставщик, фирма-производитель, деталь) фиксируется.
Виды запросов в информационной системе:
1. Получить перечень и общее число поставщиков определенной категории, поставляющих указанный вид товара, либо поставивших указанный товар в объеме, не менее заданного за определенный период.
2. Получить сведения о конкретном виде деталей: какими поставщиками поставляется, их расценки, время поставки.
3. Получить перечень и общее число покупателей, купивших указанный вид товара за некоторый период, либо сделавших покупку товара в объеме, не менее указанного.
4. Получить перечень, объем и номер ячейки для всех деталей, хранящихся на складе.
5. Вывести в порядке возрастания десять самых продаваемых деталей и десять самых "дешевых" поставщиков.
6. Получить среднее число продаж на месяц по любому виду деталей.
7. Получить долю товара конкретного поставщика в процентах, деньгах, единицах от всего оборота магазина прибыль магазина за указанный период.
8. Получить накладные расходы в процентах от объема продаж.
9. Получить перечень и общее количество непроданного товара на складе за определенный период (залежалого) и его объем от общего товара в процентах.
10. Получить перечень и общее количество бракованного товара, пришедшего за определенный период и список поставщиков, поставивших товар.
11. Получить перечень, общее количество и стоимость товара, реализованного за конкретный день.
12. Получить кассовый отчет за определенный период.
13. Получить инвентаризационную ведомость.
14. Получить скорость оборота денежных средств, вложенных в товар (как товар быстро продается).
15. Подсчитать сколько пустых ячеек на складе и сколько он сможет вместить товара.
16. Получить перечень и общее количество заявок от покупателей на ожидаемый товар, подсчитать на какую сумму даны заявки.
