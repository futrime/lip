package cmdlipconfig

import (
	"flag"
	"fmt"
	"reflect"
	"strconv"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/olekukonko/tablewriter"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip config [options]
  lip config [<key> [<value>]]

Description:
  Manage configuration.
  
  - If no arguments are specified, list all configuration.
  - If a key is specified, print the value of the key.
  - If a key and a value are specified, set the value of the key.

Options:
  -h, --help                  Show help.
`

func Run(ctx *context.Context, args []string) error {
	flagSet := flag.NewFlagSet("cache", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	if err := flagSet.Parse(args); err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	switch flagSet.NArg() {
	case 0:
		showAllConfig(ctx)

	case 1:
		if err := showConfig(ctx, flagSet.Arg(0)); err != nil {
			return fmt.Errorf("failed to show config: %w", err)
		}

	case 2:
		if err := setConfig(ctx, flagSet.Arg(0), flagSet.Arg(1)); err != nil {
			return fmt.Errorf("failed to set config: %w", err)
		}

	default:
		return fmt.Errorf("too many arguments")
	}

	return nil
}

func convertConfigToMap(config context.Config) map[string]interface{} {
	value := reflect.ValueOf(config)
	typeOfValue := value.Type()

	configMap := make(map[string]interface{})
	for i := 0; i < value.NumField(); i++ {
		configMap[typeOfValue.Field(i).Name] = value.Field(i).Interface()
	}

	return configMap
}

func convertStringToType(str string, targetType reflect.Type) (interface{}, error) {
	switch targetType.Kind() {
	case reflect.String:
		return str, nil

	case reflect.Int, reflect.Int8, reflect.Int16, reflect.Int32, reflect.Int64:
		intValue, err := strconv.ParseInt(str, 10, targetType.Bits())
		if err != nil {
			return nil, err
		}
		value := reflect.New(targetType).Elem()
		value.SetInt(intValue)
		return value.Interface(), nil

	case reflect.Uint, reflect.Uint8, reflect.Uint16, reflect.Uint32, reflect.Uint64:
		uintValue, err := strconv.ParseUint(str, 10, targetType.Bits())
		if err != nil {
			return nil, err
		}
		value := reflect.New(targetType).Elem()
		value.SetUint(uintValue)
		return value.Interface(), nil

	case reflect.Float32, reflect.Float64:
		floatValue, err := strconv.ParseFloat(str, targetType.Bits())
		if err != nil {
			return nil, err
		}
		value := reflect.New(targetType).Elem()
		value.SetFloat(floatValue)
		return value.Interface(), nil

	case reflect.Bool:
		boolValue, err := strconv.ParseBool(str)
		if err != nil {
			return nil, err
		}
		value := reflect.New(targetType).Elem()
		value.SetBool(boolValue)
		return value.Interface(), nil

	default:
		return nil, fmt.Errorf("unsupported type: %v", targetType)
	}
}

func setConfig(ctx *context.Context, key string, value string) error {
	field := reflect.ValueOf(ctx.Config()).Elem().FieldByName(key)
	if !field.IsValid() {
		return fmt.Errorf("no such key: %v", key)
	}

	if !field.CanSet() {
		return fmt.Errorf("cannot set key: %v", key)
	}

	fieldType := field.Type()
	structValueInterface, err := convertStringToType(value, fieldType)
	if err != nil {
		return fmt.Errorf("failed to convert value to type: %w", err)
	}
	structValue := reflect.ValueOf(structValueInterface)

	if structValue.Type().ConvertibleTo(fieldType) {
		field.Set(structValue.Convert(fieldType))

	} else {
		return fmt.Errorf("cannot convert value to type: %v", fieldType)
	}

	if err := ctx.SaveConfigFile(); err != nil {
		return fmt.Errorf("failed to save config file: %w", err)
	}

	return nil
}

func showAllConfig(ctx *context.Context) {
	tableData := make([][]string, 0)
	for key, value := range convertConfigToMap(*ctx.Config()) {
		tableData = append(tableData, []string{key, fmt.Sprintf("%v", value)})
	}

	tableString := &strings.Builder{}
	table := tablewriter.NewWriter(tableString)
	table.SetHeader([]string{
		"Key", "Value",
	})

	for _, row := range tableData {
		table.Append(row)
	}

	table.Render()

	fmt.Print(tableString.String())
}

func showConfig(ctx *context.Context, key string) error {
	configMap := convertConfigToMap(*ctx.Config())

	if value, ok := configMap[key]; ok {
		fmt.Printf("%v\n", value)

	} else {
		return fmt.Errorf("no such key: %v", key)
	}

	return nil
}
