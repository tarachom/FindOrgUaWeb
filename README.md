# Внутрішній проміжний вебсервер
<b>Внутрішній проміжний вебсервер для видачі контенту </b> | .net 8, Linux, Windows <br/>

Даний вебсервер віддає контент основному вебсерверу на [Erlang](https://github.com/tarachom/ErlangWeb)<br/>
Працює по протоколу http і привязаний до локальної машини (http://localhost:8082)<br/>
Віддає зразу готовий html контент, а все інше бере на себе Erlang.

Наприклад

    Новини - http://localhost:8082/news
    Особистості - http://localhost:8082/personality