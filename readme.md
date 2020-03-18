# Инструкция по установе на Raspberry

## 1. Установить библиотеки от которых зависит net core:

	sudo apt-get update
	sudo apt-get install curl libunwind8 gettext apt-transport-https

### 1.2  Скопировать архив с программой
Скачиваем проект с гиталаба

 	wget  http://gitlab.elkam2.local/Frolov/WFQYDB/uploads/6c9f3708ef034ba5ba8d3f9d6ad8afd8/converter.tgz

Распаковать в папку **~**

	cd ~
	tar xvf converter.tgz
	rm converter.tgz

## 2. Настроить права на запуск

	cd ~/Converter
	sudo chmod +x WfqydbModbusGateway
	sudo chmod +x *.so
	...

# 3. Настроить автозапуск
В файл **/etc/rc.local** добавить строчку запуска:

	/home/pi/Converter/WfqydbModbusGateway >> /dev/null &

Файл автозапуска можно редактировать только от имени суперпользователя.

# 4. Проверка
- Перезагрузить raspbery.
- Прочитать по modbus первые 10 input регистров, данные должны быть похожи на правду согласно карты регистров.
